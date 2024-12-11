from server import AsyncServer
from aiohttp import web

def main():
    server = AsyncServer()
    web.run_app(server.app, host="0.0.0.0", port=52)

if __name__ == "__main__":
    main()
