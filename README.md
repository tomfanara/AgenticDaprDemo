# Agentic Dapr Demo Application
A demo application employing domain driven design, Dapr (Distributed Application Runtime) and an Agentic architecture with microagents. The system will use Dapr Workflows as the orchestrator in the agentic patterns. Each microagent will have semantic memory, plugins, planners, personas (Khloe, Jenny and Carlos) and memories. The microagents will assist the human chat by responding to prompts with both user and system prompts. The microagents will be feature extensions of microservices with in a bounded context. The bounded context will set the boundaries of the microagents funtionality. 

Knowledge References:
- Domain Driven Design
- Distributed Aplication Runtime (Dapr)
- Agentic Architecture and related patterns
- Semantic Kernel


Intro:
In order for AI Agents (an intelligent application acting on behalf of a human business domain such as accounting, sales and marketing) to work in context they should follow domain driven design principals.
They should follow all the domain contextual relationships and live within the boundaries of there respective bounded context. 

In agentic AI architectures an orchestrator is needed utilizing a saga or orchestrator design pattern. (verus choreopgrapher however both should be considered)
The orchestrator is the central manager of the agentic chat and coordinates messaging between microagents. A microagent follows the microservices pattern of a ganular application that centers around a specific domain model.

More can be elaborated on best practices design for microagents but its recommended that a vertical slice architecture as used inmicroservices will suffice for now. A microagent could be a feature onto a microservice as well in order not to over ganularize domain expertise and functionality.
