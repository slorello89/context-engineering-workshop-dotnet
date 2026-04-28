## Context Engineering Workshop for .NET

This repository contains the ASP.NET Core / Semantic Kernel version of the Context Engineering Workshop. The workshop is organized around the dotnet implementation in `backend-dotnet-layer/`.

`main` is the completed reference implementation. Participants should normally work through the lab branches instead of starting from `main`.

## Workshop Flow

Each lab now has both a `starter` branch and a `solution` branch:

| Lab | Starter | Solution | Focus |
| --- | --- | --- | --- |
| 1 | [`lab-1-starter`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-1-starter) | [`lab-1-solution`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-1-solution) | Base app setup |
| 2 | [`lab-2-starter`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-2-starter) | [`lab-2-solution`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-2-solution) | Short-term memory |
| 3 | [`lab-3-starter`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-3-starter) | [`lab-3-solution`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-3-solution) | PDF ingestion and knowledge-base storage |
| 4 | [`lab-4-starter`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-4-starter) | [`lab-4-solution`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-4-solution) | Basic RAG with knowledge-base retrieval |
| 5 | [`lab-5-starter`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-5-starter) | [`lab-5-solution`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-5-solution) | Long-term user memory retrieval |
| 6 | [`lab-6-starter`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-6-starter) | [`lab-6-solution`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-6-solution) | Query compression and reranking |
| 7 | [`lab-7-starter`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-7-starter) | [`lab-7-solution`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-7-solution) | Few-shot prompting |
| 8 | [`lab-8-starter`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-8-starter) | [`lab-8-solution`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-8-solution) | Token-window management |
| 9 | [`lab-9-starter`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-9-starter) | [`lab-9-solution`](https://github.com/slorello89/context-engineering-workshop-dotnet/tree/lab-9-solution) | Semantic caching |

How to use the branches:

1. Check out `lab-N-starter`.
2. Follow that branch `README.md` and add the code described there.
3. Compare your result with `lab-N-solution`.
4. Move on to `lab-(N+1)-starter`.

## Running the App

The repository runs Redis support services through `docker-compose.yaml`. The ASP.NET Core backend runs separately and serves the built React frontend.

```bash
docker compose up -d
cd frontend-layer
npm install
npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

Default endpoints:

- frontend and API via ASP.NET Core: `http://localhost:8081`
- health endpoint: `http://localhost:8081/health`
- Redis Insight: `http://localhost:5540`

Environment setup:

```bash
cp .env.example .env
```

At minimum, set:

```bash
OPENAI_API_KEY=your-openai-api-key
AGENT_MEMORY_SERVER_URL=http://localhost:8000
```

The backend automatically loads the repo-root `.env` file when it starts.

## Repo Layout

- `backend-dotnet-layer/`: ASP.NET Core Web API, Semantic Kernel integration, memory/retrieval services
- `frontend-layer/`: React workshop UI
- `docker-compose.yaml`: Redis support services for the workshop
- `backend-dotnet-layer/Assets/ms-marco-MiniLM-L-6/`: local ONNX reranker assets

## 🎯 What You've Built

### Complete Context Engineering System

Your application now implements a comprehensive context engineering solution with:

![workshop-complete.png](images/workshop-complete.png)

## 📚 Context Engineering Techniques Implemented

### 1. **Memory Architectures** (Labs 2 & 5)
- **Technique**: Hierarchical Memory Systems
- **Implementation**: Dual-layer memory with short-term (conversation) and long-term (persistent) storage
- **Reference**: [Memory-Augmented Neural Networks](https://arxiv.org/abs/1410.3916)
- **Benefits**:
   - Maintains conversation coherence
   - Preserves user preferences across sessions
   - Enables personalized interactions

### 2. **Retrieval-Augmented Generation (RAG)** (Labs 3 & 4)
- **Technique**: Dynamic Context Injection
- **Implementation**: Vector-based semantic search with document chunking
- **Reference**: [RAG: Retrieval-Augmented Generation](https://arxiv.org/abs/2005.11401)
- **Benefits**:
   - Access to external knowledge
   - Reduced hallucination
   - Up-to-date information retrieval

### 3. **Query Optimization** (Lab 6)
- **Technique**: Query Compression and Expansion
- **Implementation**: LLM-based query reformulation for better retrieval
- **Reference**: [Query Expansion Techniques](https://dl.acm.org/doi/10.1145/3397271.3401075)
- **Benefits**:
   - Improved retrieval accuracy
   - Reduced noise in search results
   - Better semantic matching

### 4. **Content Reranking** (Lab 6)
- **Technique**: Cross-Encoder Reranking
- **Implementation**: ONNX-based similarity scoring with MS MARCO models
- **Reference**: [Dense Passage Retrieval](https://arxiv.org/abs/2004.04906)
- **Benefits**:
   - Higher relevance in retrieved content
   - Reduced context pollution
   - Better answer quality

### 5. **Few-shot Learning** (Lab 7)
- **Technique**: In-Context Learning (ICL)
- **Implementation**: Example-based prompting in system messages
- **Reference**: [Language Models are Few-Shot Learners](https://arxiv.org/abs/2005.14165)
- **Benefits**:
   - Consistent output format
   - Better instruction following
   - Reduced prompt engineering effort

### 6. **Token Management** (Lab 8)
- **Technique**: Sliding Window Attention
- **Implementation**: Dynamic pruning with token count estimation
- **Reference**: [Efficient Transformers](https://arxiv.org/abs/2009.06732)
- **Benefits**:
   - Prevents context overflow
   - Maintains conversation flow
   - Optimizes token usage

### 7. **Semantic Caching** (Lab 9)
- **Technique**: Vector Similarity Caching
- **Implementation**: RedisVL semantic cache with embedding-based matching
- **Reference**: [Semantic Caching for LLMs](https://arxiv.org/html/2504.02268v1)
- **Benefits**:
   - 40-60% reduction in LLM calls
   - Sub-100ms response times for cached queries
   - Significant cost savings

## 🔧 Technology Stack Mastered

### Core Technologies
- **.NET 9 / ASP.NET Core**: Web API host and static-asset serving
- **Semantic Kernel**: Chat orchestration and OpenAI integration
- **React**: Workshop frontend

### AI/ML Components
- **OpenAI chat models**: LLM responses and query compression
- **ONNX reranking**: Local relevance scoring
- **Vector search**: Semantic retrieval through the Agent Memory Server
- **RedisVL**: Semantic router and semantic cache building blocks

### Infrastructure
- **Docker Compose**: Redis support services
- **Redis Agent Memory Server**: Working memory and long-term memory APIs
- **Redis Insight**: Local inspection and debugging

## 🎓 Advanced Concepts Learned

1. **Context Window Optimization**: Balancing information density with token limits
2. **Semantic Similarity**: Understanding and implementing vector-based search
3. **Prompt Engineering**: Crafting effective system prompts with examples
4. **Memory Hierarchies**: Designing multi-tier memory systems
5. **Query Understanding**: Reformulating user intent for better retrieval
6. **Cache Strategies**: Implementing intelligent caching with semantic matching
7. **Token Economics**: Optimizing cost vs. performance in LLM applications

## 🚀 Next Steps for Your Journey

### Immediate Enhancements

#### 1. **Implement Conversation Summarization**
```csharp
public Task<string> SummarizeConversationAsync(
    IReadOnlyList<string> turns,
    CancellationToken cancellationToken)
{
    // Use the chat model to create a compact summary
    // Store the summary as long-term memory
    // Trim or replace older short-term history
    throw new NotImplementedException();
}
```

#### 2. **Add Multi-Modal Support**
- Integrate image processing with Semantic Kernel
- Add support for PDF charts and diagrams
- Implement audio transcription for voice queries

#### 3. **Enhance Memory Management**
- Implement memory importance scoring
- Add memory consolidation strategies
- Create user-controlled memory editing

### Advanced Features

#### 1. **Implement Agents and Tools**
```csharp
public sealed class AssistantTools
{
    [KernelFunction("search_web")]
    public string SearchWeb(string query)
    {
        throw new NotImplementedException();
    }

    [KernelFunction("calculate")]
    public string Calculate(string expression)
    {
        throw new NotImplementedException();
    }
}
```

#### 2. **Implement Hybrid Search**
- Combine vector search with keyword search
- Add metadata filtering for better precision
- Implement BM25 + dense retrieval fusion

### Production Considerations

#### 1. **RAG Observability and Monitoring**
```csharp
public sealed class RetrievalDiagnostics
{
    private readonly ILogger<RetrievalDiagnostics> _logger;

    public RetrievalDiagnostics(ILogger<RetrievalDiagnostics> logger)
    {
        _logger = logger;
    }

    public void LogRetrieval(string query, string route, int candidateCount)
    {
        _logger.LogInformation(
            "Query '{Query}' routed to '{Route}' with {CandidateCount} candidates",
            query,
            route,
            candidateCount);
    }
}
```

Semantic Kernel and ASP.NET Core logging provide a straightforward place to instrument chat, retrieval, reranking, and cache activity.

#### 2. **Security and Privacy**
- Implement PII detection and masking
- Add conversation encryption
- Create audit logs for compliance
- Implement user consent management

#### 3. **Scale and Performance**
- Implement distributed caching with Redis Cluster
- Add connection pooling for LLM calls
- Use async processing for document ingestion
- Implement circuit breakers for resilience

### Learning Resources

#### Research Papers
- [Attention Is All You Need](https://arxiv.org/abs/1706.03762)
- [BERT: Pre-training of Deep Bidirectional Transformers](https://arxiv.org/abs/1810.04805)
- [Constitutional AI](https://arxiv.org/abs/2212.08073)

#### Online Courses
- [CS324 - Large Language Models (Stanford)](https://stanford-cs324.github.io/winter2022/)
- [Full Stack LLM Bootcamp](https://fullstackdeeplearning.com/llm-bootcamp/)
- [Semantic Caching for AI Agents](https://www.deeplearning.ai/short-courses/semantic-caching-for-ai-agents/)

### Community and Contribution

#### Join the Community
- [Semantic Kernel](https://github.com/microsoft/semantic-kernel)
- [Redis Developer Community](https://discord.gg/redis)

#### Contribute Back
- Share your improvements as PRs
- Write blog posts about your learnings
- Create video tutorials
- Help others in community forums

## 🏅 Certification of Completion

You've demonstrated proficiency in:
- ✅ Context Window Management
- ✅ Memory System Architecture
- ✅ Retrieval-Augmented Generation
- ✅ Query Optimization Techniques
- ✅ Semantic Caching Strategies
- ✅ Token Economics and Management
- ✅ Production-Ready AI Applications

## 🙏 Acknowledgments

This workshop was made possible by:
- the Semantic Kernel and ASP.NET Core communities
- the Redis Developer Relations team
- All workshop participants and contributors

## 📬 Feedback and Support

- **Workshop Issues**: update this link after the new repository is published
- **Improvements**: PRs are welcome!

---

**Thank you for joining us on this Context Engineering journey!**

You're now equipped with the knowledge and tools to build sophisticated, production-ready AI applications. The future of context-aware AI is in your hands. Go forth and build amazing things! 🚀

---
