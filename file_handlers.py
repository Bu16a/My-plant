import logging
import os
import tempfile
import asyncio
from aiohttp import web
from aiohttp.web_request import Request
from typing import Dict

file_lock = asyncio.Lock()

class FileHandler:
    def __init__(self):
        self.file_lock = file_lock
        self.save_folder = "uploaded_images"
        os.makedirs(self.save_folder, exist_ok=True)

    async def save_image_with_id(self, request: Request) -> web.Response:
        try:
            reader = await request.multipart()
            file_field = await reader.next()

            if not file_field or file_field.name != "file":
                return web.json_response({"error": "Parameter 'file' is required"}, status=400)

            with tempfile.NamedTemporaryFile(delete=False) as temp_file:
                while True:
                    chunk: bytes = await file_field.read_chunk()
                    if not chunk:
                        break
                    temp_file.write(chunk)
                temp_file_path = temp_file.name

            text_field = await reader.next()
            identifier: str = await text_field.text()
            if not identifier.strip():
                return web.json_response({"error": "Identifier cannot be empty"}, status=400)

            save_path: str = os.path.join(self.save_folder, f"{identifier}.jpg")
            os.replace(temp_file_path, save_path)

            return web.json_response({"message": f"File saved successfully as {save_path}"})

        except Exception as e:
            logging.error(f"Ошибка в save_image_with_id: {e}")
            return web.json_response({"error": f"Error saving file: {e}"}, status=500)

    async def get_image_by_id(self, request: Request) -> web.Response:
        async with self.file_lock:
            try:
                data: Dict = await request.json()
                identifier: str = data.get("identifier")
                if not identifier:
                    return web.json_response({"error": "Parameter 'identifier' is required"}, status=400)

                image_path: str = os.path.join(self.save_folder, f"{identifier}.jpg")
                if not os.path.exists(image_path):
                    return web.json_response({"error": f"Image with identifier '{identifier}' not found"}, status=404)

                return web.FileResponse(image_path)

            except Exception as e:
                return web.json_response({"error": f"Error retrieving image: {e}"}, status=500)
