import g4f
from g4f.client import Client
import google.generativeai as genai
import os
from dotenv import load_dotenv
import time
import requests


class AIModelHandler:
    def __init__(self):
        load_dotenv()
        genai.configure(api_key=os.getenv("API_KEY"))
        self.cx = os.getenv("MY_CX")
        self.api_search = os.getenv("MY_SEARCH_API_KEY")
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

    def get_first_image_google(self, query):
        # URL для Google Custom Search API
        search_url = "https://www.googleapis.com/customsearch/v1"
        params = {
            "q": query,
            "cx": self.cx,
            "key": self.api_search,
            "searchType": "image",
            "num": 1,
        }

        response = requests.get(search_url, params=params)
        if response.status_code != 200:
            print(f"Ошибка при запросе к API: {response.status_code}")
            return None

        data = response.json()
        if "items" in data and len(data["items"]) > 0:
            return data["items"][0]["link"]
        else:
            print("Картинка не найдена")
            return None
