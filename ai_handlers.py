import g4f
from g4f.client import Client
import google.generativeai as genai
import os
from dotenv import load_dotenv
import time
from PIL import Image

class AIModelHandler:
    def __init__(self):
        load_dotenv()
        genai.configure(api_key=os.getenv("API_KEY"))
        self.client = Client()

    async def get_gpt_response_g4f(self, user_query: str, model: g4f.models.default = g4f.models.default) -> str:
        for attempt in range(3):
            try:
                response = await self.client.chat.completions.async_create(
                    model=model,
                    messages=[{"role": "user", "content": user_query}]
                )
                if response:
                    return response.choices[0].message.content
            except Exception as e:
                print(f"Attempt {attempt + 1} with g4f failed: {e}")
        return "Failed to get a response from g4f after several attempts."

    @staticmethod
    def analyze_image_with_prompt(image_path: str, prompt: str) -> str:
        try:
            start_time = time.time()
            uploaded_file = genai.upload_file(image_path)
            upload_time = time.time() - start_time

            model = genai.GenerativeModel("gemini-1.5-flash")
            start_generate = time.time()
            result = model.generate_content([uploaded_file, "\n\n", prompt])
            generate_time = time.time() - start_generate

            total_time = time.time() - start_time
            print(f"Upload time: {upload_time}s, Generate time: {generate_time}s, Total time: {total_time}s")

            return result.text if hasattr(result, "text") else "Image analysis failed or contains no text."
        except Exception as e:
            return f"An error occurred during image analysis: {e}"

    @staticmethod
    def get_text_response(prompt: str) -> str:
        try:
            model = genai.GenerativeModel("gemini-1.5-flash")
            result = model.generate_content([prompt])
            return result.text if hasattr(result, "text") else "The model's response contains no text."
        except Exception as e:
            return f"An error occurred while processing the text request: {e}"

    @staticmethod
    def compress_image(input_path: str, output_path: str, quality: int = 85, max_size: tuple = (1024, 1024)) -> None:
        try:
            with Image.open(input_path) as img:
                img.thumbnail(max_size, Image.LANCZOS)
                img.save(output_path, format="JPEG", quality=quality, optimize=True)
        except Exception as e:
            print(f"Error during image compression: {e}")
