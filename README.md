# 1. Agentic Dapr Demo Application (summary)
A demo application employing domain driven design, Dapr (Distributed Application Runtime) and an agentic architecture with micro agents. The system will use Dapr Workflows as the orchestrator and choreographer in the agentic system. Each micro agent will have semantic memory, plugins, planners, personas (Khloe-supervisor, Jenny and Carlos) and memories. The micro agents will assist the human completion chat by responding to prompts with user, meta (personas) and system prompts. The micro agents will be feature extensions of microservices with in a bounded context. The bounded context will limit the behavior of the micro agents funtionality as virtual domain experts. 
## 2. Purpose 
This demo will serve as a means to observe, develop and improve how Dapr Workflows and components work with agentic architectures. Agentic architectures are still in early stages but natural language based development is growing at an incredible rate. Upon its completion it can be used to show case Dapr integration in agentic architectures. 

The following Dapr building blocks will be included in the demo
      - Configuration
      - Workflows
      - State stores

We will omit gateways, authentication and middleware to keep focus on the above purpose.
## 3. References:
- Domain Driven Design
- Distributed Application Runtime (Dapr)
- Agentic Architecture and related patterns
- Microsoft Semantic Kernel
- Ollama LLM Framework
- Andrew Ng - Agentic Design Patterns
    - Reflection pattern
    - Tools pattern
    - Planner pattern
    - Multi Agent pattern
## 4. Project Description:
### Domain Model
In order for micro agents of an agentic architecture to work in context they should follow domain driven design principals.
They should follow all the domain context relationships and live within the boundaries of their respective bounded context. 

The Demo Domain Driven Design diagram below is a basic domain with its various bounded context. We will keep it simple with only 3 contexts and a basic ubiquitous language.

  1. Accounting/Human Resources domain (still Accounting in code) (Khloe) - Employee resourcing. They are the managers of all operations. Use a SQL database for receivables and payables.
  2. Sales (Carlos) - Uses document storage.
  3. Inventory (Jenny) - Use SQL database for storage
  4. Shipping (Jenny) - Use a SQL database for data storage of shipped widgets.
  5. Receiving (Jenny) - Use a document corpus for storing received widget stock.

This is a very familiar and general domain model with its related contexts. The U/D denotes an upstream/downstream relationship between contexts. PL denotes a Published Language as in a Sales Force API. CS denotes Customer Supplier relationship and of course there is a Partnership relationship between Accounting and Inventory. The Xray icon denotes a Big Ball of Mudd or a legacy inventory system. We have many of the hybrid conditions found in most enterprises.

In agentic AI architectures an orchestrator is needed utilizing a saga or orchestrator design pattern. 
The orchestrator is the central manager of the agentic chat and coordinates chat completion messaging between micro agents. A micro agent follows the microservices pattern that centers around a specific domain model. There are 2 types of chats Task Chat and Group Chat. Task chat is a routing slip pattern that is orchestrated by dapr agents. The prompt is reviewed by workflow agents then associated domains are sequentially passed a task to complete. Group chat relies on an orchestrator as well but in this case messsages are routed only to an agent in the group at a time instead of a routing pattern. 

More can be elaborated on best practices design for micro agents but its recommended that a vertical slice architecture as used in microservices will suffice. A micro agent can be a feature of a microservice as to not over granularize as a separate service.
### Agentic Design Patterns
The following diagrams depict the overall architectural elements and information flow. The first is a series of diagrams depicting the 4 main agentic patterns by Andrew Ng. The demo will be implementing all 4 to a limited extent.
![Alt text](agentic-design-patterns.png "agentic design patterns image for demo")
### Agentic Architecture with MicroAgents
The following diagram illustrates the scope of the demo and overall architecture. It is not a production worthy system as gateways, authorization, encryption and core business domains are omitted for purpose of clarity.
![Alt text](agenticarchdemo2.png "agentic architecture image for demo")
The following is the demo software design. The client app is a Angular Nubular chat famework application with a web sockets hub connection. It will also have 4 additional .NET Core APIs and microservices.
There's two branches to run the app from. 1. features/workflow and ABC-Wholesale-Assist

- ClientApp - Angular application. Main chat initiation and dialogue, starts workflows and keeps chat conversation.
- Workflow.API (Management.API for ABC Wholesale): Orchestrator pattern that maintains conversation state, app crash resilience and external calls.
- Accounting.API: Contains a micro agent feature that acts on behalf of the domain. Its domain model is persisted uses an SQL database.
- Inventory.API: Contains a micro agent feature that acts on behalf of the domain. Its domain model is persisted by a NoSQL database.
- Sales.API: Contains a micro agent feature that acts on behalf of the domain. Its domain model is persisted by a document corpus.
- ChatHub.API(Notifications.API for ABC Wholesale): web sockets hub establishing communication channels with client app and agents.
  
### Demo Software Design
The above APIs will run in Dapr in Docker and dotnet core web applications with respective ports. The Angular chat app will start the workflows both orchestration and choreography. The prompts will make calls via the web socket hub microservice to the work flow and back to the chat. The micro agents contain their own plugins, personas, planners and memories. The Dapr wait for external call we be used to call back to the chat client from a micro agent downstream. Finally documents will be requested by the supervisor micro agent from the other agents and saved to a local folder.

Its important to note that the workflow will first implement query rewriting as the basis of contacting and orchestrating micro agents. Finally at the end it will re-query the aggregated results and refine the prompt request.
# 5. Installation
      - Visual Studio 2022 (with powershell tools extension)
      - Docker Desktop (latest)
      - Dapr Client and Runtime 1.14
      - Ollama (install/run phi3, llava-phi3, llama3.1, llama3, orca-mini, wizardlm2, deepseek-r1:1.5b)
      - NVM, NPM and Visual Studio Code. Download the chat app here. https://github.com/koulyves288/crs-assist
# 6. Usage
      - run the demo app (backend) in Visual Studio 2022
      - run the chat app from VS Code after building in NPM v20 (ng build)
      - run ng serve
      - make sure your chat app is connected to the hub
      - you might have to adjust the ports on client and backend
# 7. Contributing
# 8. License
# 9. Contact Information
# 10. FAQ
# 11. Credit
# 12. Changelog
