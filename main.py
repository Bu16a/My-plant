import logging
import os
from container import AppContainer
from aiohttp import web

def main():
    log_file_path = os.path.join(os.getcwd(), "server.log")
    os.makedirs(os.path.dirname(log_file_path), exist_ok=True)
    open(log_file_path, "a").close()
    logging.basicConfig(
        filename=log_file_path,
        filemode="a",
        level=logging.DEBUG,
        format="%(asctime)s - %(levelname)s - %(message)s")


    container = AppContainer()
    server = container.async_server()
    web.run_app(server.app, host="0.0.0.0", port=52)

if __name__ == "__main__":
    main()