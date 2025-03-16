using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeminiRAGChat : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private TextAsset[] knowledgeBaseFiles; // Assign your text assets in Inspector
    [SerializeField] private GameObject userMessagePrefab;
    [SerializeField] private GameObject aiMessagePrefab;
    [SerializeField] private Transform chatContent; // Assign the Content of the ScrollRect

    private string apiKey = "Gemini-APIkey"; // Replace with your Gemini API key
    private string geminiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    private string embeddingUrl = "https://generativelanguage.googleapis.com/v1beta/models/embedding-001:embedContent"; // For vector embeddings
    private List<GeminiMessage> messages;
    private bool isWaitingForResponse = false;
    
    // RAG specific variables
    private List<Document> knowledgeBase = new List<Document>();
    private int maxRetrievedDocs = 3; // Number of documents to retrieve

    [System.Serializable]
    public class Document
    {
        public string content;
        public List<float> embedding;
        public string title;
    }

    void Start()
    {
        // Initialize conversation history
        messages = new List<GeminiMessage>
        {
            new GeminiMessage {
                role = "user",
                parts = new List<GeminiPart> {
                    new GeminiPart {
                        text = "I am working on an educational game called READAROO, specifically designed for dyslexic children aged 6-12. The game aims to help children improve their reading and language skills through interactive and engaging mini-games. In particular, one of the games, 'Letter Sound Match-Up,' helps players match audio letter sounds to visual groups. This game is part of a larger educational platform with 6 mini-games that encourage learning through play."
                    }
                }
            },
            new GeminiMessage {
                role = "model",
                parts = new List<GeminiPart> {
                    new GeminiPart {
                        text = "I understand! I'll help players with READAROO, focusing on the educational aspects while keeping responses simple and encouraging."
                    }
                }
            }
        };

        sendButton.onClick.AddListener(GetResponse);

        // Add initial message to chat
        AddMessageToChat("I'm Your AI Assistant for READAROO. How can I help you today?", false);
        
        // Initialize the knowledge base at startup
        StartCoroutine(InitializeKnowledgeBase());
    }

    private IEnumerator InitializeKnowledgeBase()
    {
        // Process each knowledge file and create embeddings
        foreach (var file in knowledgeBaseFiles)
        {
            // Split the text into chunks (paragraphs or sections)
            string[] chunks = file.text.Split(
                new[] { "\n\n", "\r\n\r\n" }, 
                System.StringSplitOptions.RemoveEmptyEntries
            );
            
            foreach (var chunk in chunks)
            {
                if (string.IsNullOrWhiteSpace(chunk)) continue;
                
                // Create a new document
                Document doc = new Document
                {
                    content = chunk,
                    title = file.name
                };
                
                // Get embedding for this chunk
                yield return StartCoroutine(GetEmbedding(doc));
                
                // Add to knowledge base
                knowledgeBase.Add(doc);
                
                // Small delay to avoid rate limiting
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        Debug.Log($"Knowledge base initialized with {knowledgeBase.Count} documents");
    }
    
    private IEnumerator GetEmbedding(Document doc)
    {
        using (HttpClient client = new HttpClient())
        {
            // Prepare request for embeddings
            string fullUrl = $"{embeddingUrl}?key={apiKey}";
            
            var requestBody = new 
            {
                model = "models/embedding-001",
                content = new 
                {
                    parts = new[] 
                    {
                        new { text = doc.content }
                    }
                }
            };
            
            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            // Send the request
            var responseTask = client.PostAsync(fullUrl, content);
            while (!responseTask.IsCompleted)
            {
                yield return null;
            }
            
            var response = responseTask.Result;
            var contentTask = response.Content.ReadAsStringAsync();
            
            while (!contentTask.IsCompleted)
            {
                yield return null;
            }
            
            string responseContent = contentTask.Result;
            
            if (response.IsSuccessStatusCode)
            {
                // Parse embedding response
                var responseObj = JsonConvert.DeserializeObject<EmbeddingResponse>(responseContent);
                doc.embedding = responseObj.embedding.values;
            }
            else
            {
                Debug.LogError($"Failed to get embedding: {responseContent}");
                // Fallback: empty embedding vector
                doc.embedding = new List<float>();
            }
        }
    }
    
    public void GetResponse()
    {
        if (string.IsNullOrEmpty(inputField.text) || isWaitingForResponse)
            return;

        isWaitingForResponse = true;
        sendButton.interactable = false;

        // Add user message to the conversation
        string userMessage = inputField.text;
        messages.Add(new GeminiMessage { 
            role = "user", 
            parts = new List<GeminiPart> { new GeminiPart { text = userMessage } }
        });

        // Update the display
        AddMessageToChat(userMessage, true);
        inputField.text = "";

        // Start the RAG process
        StartCoroutine(RAGProcess(userMessage));
    }
    
    private IEnumerator RAGProcess(string userMessage)
    {
        // Step 1: Get embedding for the query
        Document queryDoc = new Document { content = userMessage };
        yield return StartCoroutine(GetEmbedding(queryDoc));
        
        // Step 2: Retrieve relevant documents
        List<Document> retrievedDocs = new List<Document>();
        
        if (queryDoc.embedding != null && queryDoc.embedding.Count > 0)
        {
            // Calculate similarity scores and retrieve top documents
            var scoredDocs = knowledgeBase
                .Where(doc => doc.embedding != null && doc.embedding.Count > 0)
                .Select(doc => new 
                { 
                    Document = doc, 
                    Score = CosineSimilarity(queryDoc.embedding, doc.embedding) 
                })
                .OrderByDescending(x => x.Score)
                .Take(maxRetrievedDocs)
                .ToList();
            
            retrievedDocs = scoredDocs.Select(x => x.Document).ToList();
            Debug.Log($"Retrieved {retrievedDocs.Count} documents for query: {userMessage}");
        }
        
        // Step 3: Create augmented prompt with retrieved content
        StringBuilder contextBuilder = new StringBuilder();
        contextBuilder.AppendLine("Using the following information to help answer the question:");
        
        foreach (var doc in retrievedDocs)
        {
            contextBuilder.AppendLine("---");
            contextBuilder.AppendLine(doc.content);
        }
        
        contextBuilder.AppendLine("---");
        contextBuilder.AppendLine("Question: " + userMessage);
        contextBuilder.AppendLine("Answer the question based on the provided information. If the information doesn't contain the answer, just say so without making up information. Keep your answer child-friendly and educational.Give answer as a one paragraph and keep it simple.keep it under 50 words");
        
        string augmentedPrompt = contextBuilder.ToString();
        
        // Step 4: Send the augmented prompt to Gemini
        yield return StartCoroutine(SendGeminiRequest(augmentedPrompt, userMessage));
    }

    private IEnumerator SendGeminiRequest(string augmentedPrompt, string originalUserMessage)
    {
        using (HttpClient client = new HttpClient())
        {
            // Prepare the URL with API key
            string fullUrl = $"{geminiUrl}?key={apiKey}";

            // Create request body
            var requestBody = new GeminiRequest
            {
                contents = new List<GeminiMessage> {
                    new GeminiMessage {
                        role = "user",
                        parts = new List<GeminiPart> { new GeminiPart { text = augmentedPrompt } }
                    }
                },
                generationConfig = new GeminiGenerationConfig
                {
                    temperature = 0.3f,
                    maxOutputTokens = 200 // Increased for more detailed responses
                }
            };

            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var responseTask = client.PostAsync(fullUrl, content);
            while (!responseTask.IsCompleted)
            {
                yield return null;
            }

            var response = responseTask.Result;
            var contentTask = response.Content.ReadAsStringAsync();
            
            while (!contentTask.IsCompleted)
            {
                yield return null;
            }
            
            string responseContent = contentTask.Result;

            if (response.IsSuccessStatusCode)
            {
                var responseObject = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);
                string aiResponse = responseObject.candidates[0].content.parts[0].text;

                // Add AI response to the conversation history (use original user message for context)
                messages.Add(new GeminiMessage { 
                    role = "model", 
                    parts = new List<GeminiPart> { new GeminiPart { text = aiResponse } }
                });

                // Update the display
                AddMessageToChat(aiResponse, false);
            }
            else
            {
                Debug.LogError("API Error: " + responseContent);
                AddMessageToChat("Error: Could not get a response. Error code: " + response.StatusCode, false);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    AddMessageToChat("Waiting 5 seconds before you can try again...", false);
                    yield return new WaitForSeconds(5f);
                }
            }

            // Reset state
            isWaitingForResponse = false;
            sendButton.interactable = true;
        }
    }

    private void AddMessageToChat(string message, bool isUser)
    {
        GameObject messagePrefab = isUser ? userMessagePrefab : aiMessagePrefab;
        GameObject messageInstance = Instantiate(messagePrefab, chatContent);
        TMP_Text messageText = messageInstance.GetComponentInChildren<TMP_Text>();
        string label = isUser ? "You: " : "Bot: ";
        messageText.text = label + message;

        // Ensure the chat scrolls to the bottom when a new message is added
        Canvas.ForceUpdateCanvases();
        var scrollRect = chatContent.GetComponentInParent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    // Helper method to calculate cosine similarity between two embeddings
    private float CosineSimilarity(List<float> a, List<float> b)
    {
        if (a.Count != b.Count) return 0;

        float dotProduct = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;

        for (int i = 0; i < a.Count; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        magnitudeA = Mathf.Sqrt(magnitudeA);
        magnitudeB = Mathf.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0) return 0;

        return dotProduct / (magnitudeA * magnitudeB);
    }

    // Classes for API requests/responses
    [System.Serializable]
    public class GeminiRequest
    {
        public List<GeminiMessage> contents;
        public GeminiGenerationConfig generationConfig;
    }

    [System.Serializable]
    public class GeminiGenerationConfig
    {
        public float temperature;
        public int maxOutputTokens;
    }

    [System.Serializable]
    public class GeminiMessage
    {
        public string role;
        public List<GeminiPart> parts;
    }

    [System.Serializable]
    public class GeminiPart
    {
        public string text;
    }

    [System.Serializable]
    public class GeminiResponse
    {
        public List<GeminiCandidate> candidates;

        [System.Serializable]
        public class GeminiCandidate
        {
            public GeminiContent content;
        }

        [System.Serializable]
        public class GeminiContent
        {
            public List<GeminiPart> parts;
            public string role;
        }
    }
    
    [System.Serializable]
    public class EmbeddingResponse
    {
        public Embedding embedding;
        
        [System.Serializable]
        public class Embedding
        {
            public List<float> values;
        }
    }
}