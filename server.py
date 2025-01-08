import json

from aiohttp import web
from aiohttp.web_request import Request
from aiohttp.web_response import Response
import g4f
import os
import requests
import tempfile
from prompts import Prompts
import re
from ai_handlers import AIModelHandler
from file_handlers import FileHandler


class AsyncServer:
    def __init__(self) -> None:
        self.app: web.Application = web.Application()
        self.model_handler: AIModelHandler = AIModelHandler()
        self.file_handler: FileHandler = FileHandler()
        self.setup_routes()

    def setup_routes(self) -> None:
        self.app.router.add_post("/", self.root)
        self.app.router.add_post("/gpt-query-g4f", self.gpt_query_g4f)
        self.app.router.add_post("/gpt-query-gemini", self.gpt_query_gemini)
        self.app.router.add_post("/image-analysis-gemini", self.image_analysis_gemini)
        self.app.router.add_post("/image-analysis-file", self.image_analysis_file)
        self.app.router.add_post("/save-image-with-id", self.file_handler.save_image_with_id)
        self.app.router.add_post("/get-image-by-id", self.file_handler.get_image_by_id)
        self.app.router.add_post('/get_gz', self.get_gz)
        self.app.router.add_post('/get-first-image_url', self.get_first_image_url)
        self.app.router.add_post("/get_info", self.get_info)
        self.app.router.add_post("/get-first-image", self.get_first_image)

    @staticmethod
    async def root(request: Request) -> Response:
        return web.json_response({"message": "Async aiohttp server is running"})

    async def get_first_image_url(self, request: Request) -> Response:
        try:
            data: dict = await request.json()
            query: str = data.get("query")
            if not query:
                return web.json_response({"error": "Parameter 'query' is required"}, status=400)

            image_url: str = self.model_handler.get_first_image_google(query)
            if not image_url:
                return web.json_response({"error": "No image found for the query"}, status=404)

            return web.json_response({"image_url": image_url})

        except Exception as e:
            return web.json_response({"error": f"Error processing request: {e}"}, status=500)

    async def get_first_image(self, request: Request) -> Response:
        try:
            data: dict = await request.json()
            query: str = data.get("query")
            if not query:
                return web.json_response({"error": "Parameter 'query' is required"}, status=400)
            image_url: str = self.model_handler.get_first_image_google(query)
            if not image_url:
                return web.json_response({"error": "No image found for the query"}, status=404)
            response = requests.get(image_url, stream=True)
            if response.status_code != 200:
                return web.json_response({"error": "Failed to download the image"}, status=400)
            content_type = response.headers.get("Content-Type", "application/octet-stream")
            return Response(body=response.content, content_type=content_type)

        except Exception as e:
            return web.json_response({"error": f"Error processing request: {e}"}, status=500)

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

    async def get_info(self, request: Request) -> Response:
        try:
            # Получаем данные из запроса
            data: dict = await request.json()
            plant_name: str = data.get("query")
            if not plant_name:
                return web.json_response({"error": "Query parameter is required"}, status=400)

            # Формируем запрос для модели
            prompt = (
                f"Предоставь информацию о растении {plant_name}. "
                "Ответ в формате валидного JSON. Пример: "
                '{"Частота полива (раз в неделю)": 2, "Объём горшка (л)": 5, "Сколько воды нужно в одном поливе (л)": 1.5, '
                '"Освещение": "Яркий рассеянный свет", "Температура (°C)": 20, "Влажность воздуха (%)": 50, '
                '"Подкормка (раз в месяц)": 1, "Интерсный факт" : "Данное растение вывели в Великобритании".}. Только JSON, ничего лишнего.'
            )

            # Получаем результат через AIModelHandler
            result: str = self.model_handler.get_text_response(prompt)

            # Логирование результата
            print(f"Raw model response: {result}")

            # Проверяем пустой результат
            if not result.strip():
                return web.json_response({"error": "Model returned an empty response"}, status=500)

            # Удаляем лишние обратные кавычки и форматируем JSON
            cleaned_result = re.sub(r"```(?:json)?\n(.*)\n```", r"\1", result, flags=re.DOTALL).strip()

            # Преобразуем результат в JSON
            try:
                plant_info = json.loads(cleaned_result)
                if not isinstance(plant_info, dict):
                    raise ValueError("Response is not a valid JSON object")
            except json.JSONDecodeError as e:
                # Логирование для отладки
                print(f"JSON parsing error: {e}")
                print(f"Cleaned result: {cleaned_result}")
                return web.json_response({"error": f"Failed to parse model response: {e}"}, status=500)

            # Возвращаем результат
            return web.json_response(plant_info)

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
