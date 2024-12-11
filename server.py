from aiohttp import web
from aiohttp.web_request import Request
from aiohttp.web_response import Response
from handlers import AIModelHandler
import g4f
import os
import requests
import tempfile
from prompts import Prompts
import re


class AsyncServer:
    def __init__(self) -> None:
        self.app: web.Application = web.Application()
        self.model_handler: AIModelHandler = AIModelHandler()
        self.setup_routes()

    def setup_routes(self) -> None:
        self.app.router.add_post("/", self.root)
        self.app.router.add_post("/gpt-query-g4f", self.gpt_query_g4f)
        self.app.router.add_post("/gpt-query-gemini", self.gpt_query_gemini)
        self.app.router.add_post("/image-analysis-gemini", self.image_analysis_gemini)
        self.app.router.add_post("/image-analysis-file", self.image_analysis_file)
        self.app.router.add_post("/save-image-with-id", self.save_image_with_id)
        self.app.router.add_post("/get-image-by-id", self.get_image_by_id)
        self.app.router.add_post('/get_gz', self.get_gz)

    @staticmethod
    async def root(request: Request) -> Response:
        return web.json_response({"message": "Async aiohttp server is running"})

    async def gpt_query_g4f(self, request: Request) -> Response:
        try:
            data: dict = await request.json()
            user_query: str = data.get("query")
            if not user_query:
                return web.json_response({"error": "Query parameter is required"}, status=400)

            model_name: str | None = data.get("model")
            model = getattr(g4f.models, model_name, g4f.models.default) if model_name else g4f.models.default

            result: str = await self.model_handler.get_gpt_response_g4f(user_query, model=model)
            if not result.strip():
                return web.json_response({"error": "Model returned an empty result"}, status=500)
            return web.json_response({"result": result.strip()})

        except Exception as e:
            return web.json_response({"error": f"Error processing request: {e}"}, status=500)

    async def get_gz(self, request: Request) -> Response:
        try:
            data: dict = await request.json()
            user_query: str = data.get("flower")
            if not user_query:
                return web.json_response({"error": "Query parameter is required"}, status=400)
            user_query = Prompts.flower_instruction + user_query

            result: str = self.model_handler.get_text_response(user_query).replace('\n', '').strip()
            return web.json_response({"hz": int(result)})

        except Exception as e:
            return web.json_response({"error": f"Error processing request: {e}"}, status=500)

    async def gpt_query_gemini(self, request: Request) -> Response:
        try:
            data: dict = await request.json()
            user_query: str = data.get("query")
            if not user_query:
                return web.json_response({"error": "Query parameter is required"}, status=400)

            result: str = self.model_handler.get_text_response(user_query)
            return web.json_response({"result": result})

        except Exception as e:
            return web.json_response({"error": f"Error processing request: {e}"}, status=500)

    async def image_analysis_gemini(self, request: Request) -> Response:
        try:
            data: dict = await request.json()
            image_url: str = data.get("image_url")
            prompt: str = data.get("prompt", Prompts.image_flower_prompt)

            if not image_url:
                return web.json_response({"error": "Parameter 'image_url' is required"}, status=400)

            response: requests.Response = requests.get(image_url, stream=True)
            if response.status_code != 200:
                return web.json_response({"error": "Failed to download image from URL"}, status=400)

            with tempfile.NamedTemporaryFile(suffix=".jpg", delete=False) as temp_file:
                temp_file.write(response.content)
                image_path: str = temp_file.name

            result: str = self.model_handler.analyze_image_with_prompt(image_path, prompt)
            os.remove(image_path)

            return web.json_response({"analysis": result})

        except requests.exceptions.MissingSchema:
            return web.json_response({"error": "Invalid URL format for 'image_url'"}, status=400)
        except Exception as e:
            return web.json_response({"error": f"Error processing image: {e}"}, status=500)

    async def image_analysis_file(self, request: Request) -> Response:
        try:
            if not request.content_type.startswith("multipart/"):
                return web.json_response({"error": "Content-Type must be multipart/form-data"}, status=400)
            reader = await request.multipart()
            field = await reader.next()
            if not field or field.name != "file":
                return web.json_response({"error": "Parameter 'file' is required"}, status=400)

            with tempfile.NamedTemporaryFile(suffix=".jpg", delete=False) as temp_file:
                while True:
                    chunk: bytes = await field.read_chunk()
                    if not chunk:
                        break
                    temp_file.write(chunk)
                image_path: str = temp_file.name

            field = await reader.next()
            prompt: str = await field.text() if field and field.name == "prompt" else Prompts.image_flower_prompt
            result: list[str] = self.model_handler.analyze_image_with_prompt(image_path, prompt).replace('\n',
                                                                                                         '').split(',')

            result = [re.sub(r'[.,\"#?!]', '', r).strip() for r in result]
            result = list(set(result))
            if 'Растений_нет' in result:
                result = []
            os.remove(image_path)

            return web.json_response(result)

        except Exception as e:
            return web.json_response({"error": f"Error processing image: {e}"}, status=500)

    @staticmethod
    async def save_image_with_id(request: Request) -> Response:
        try:
            reader = await request.multipart()
            file_field = await reader.next()
            if file_field.name != "file":
                return web.json_response({"error": "Parameter 'file' is required"}, status=400)

            with tempfile.NamedTemporaryFile(delete=False) as temp_file:
                while True:
                    chunk: bytes = await file_field.read_chunk()
                    if not chunk:
                        break
                    temp_file.write(chunk)
                temp_file_path: str = temp_file.name

            text_field = await reader.next()
            identifier: str = await text_field.text()
            if not identifier.strip():
                return web.json_response({"error": "Identifier cannot be empty"}, status=400)

            save_folder: str = "uploaded_images"
            os.makedirs(save_folder, exist_ok=True)
            save_path: str = os.path.join(save_folder, f"{identifier}.jpg")
            os.replace(temp_file_path, save_path)

            return web.json_response({"message": f"File saved successfully as {save_path}"})

        except Exception as e:
            return web.json_response({"error": f"Error saving file: {e}"}, status=500)

    @staticmethod
    async def get_image_by_id(request: Request) -> Response:
        try:
            data: dict = await request.json()
            identifier: str = data.get("identifier")
            if not identifier:
                return web.json_response({"error": "Parameter 'identifier' is required"}, status=400)

            save_folder: str = "uploaded_images"
            image_path: str = os.path.join(save_folder, f"{identifier}.jpg")
            if not os.path.exists(image_path):
                return web.json_response({"error": f"Image with identifier '{identifier}' not found"}, status=404)

            return web.FileResponse(image_path)

        except Exception as e:
            return web.json_response({"error": f"Error retrieving image: {e}"}, status=500)


if __name__ == "__main__":
    server = AsyncServer()
    web.run_app(server.app, host="0.0.0.0", port=52)
