from dependency_injector import containers, providers
from ai_handlers import AIModelHandler
from file_handlers import FileHandler
from server import AsyncServer

class AppContainer(containers.DeclarativeContainer):
    wiring_config = containers.WiringConfiguration(modules=["server", "tests"])
    ai_model_handler = providers.Singleton(AIModelHandler)
    file_handler = providers.Singleton(FileHandler)
    async_server = providers.Factory(AsyncServer)
