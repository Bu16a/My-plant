from container import AppContainer
from aiohttp import web

def main():
    container = AppContainer()
    server = container.async_server()
    web.run_app(server.app, host="0.0.0.0", port=52)

if __name__ == "__main__":
    main()
