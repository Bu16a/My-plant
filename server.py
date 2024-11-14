from aiohttp import web
from handlers import get_gpt_response_g4f, analyze_image_with_prompt, get_text_response
import g4f
import os
import requests
import tempfile

app = web.Application()

async def root(request):
    return web.json_response({"message": "Async aiohttp server is running "})

async def gpt_query_g4f(request):
    try:
        data = await request.json()
        user_query = data.get("query")
        if not user_query:
            return web.json_response({"error": "Query parameter is required"}, status=400)

        model_name = data.get("model")
        model = getattr(g4f.models, model_name, g4f.models.default) if model_name else g4f.models.default

        result = await get_gpt_response_g4f(user_query, model=model)
        return web.json_response({"result": result})

    except Exception as e:
        return web.json_response({"error": f"Error processing request: {e}"}, status=500)

async def gpt_query_gemini(request):
    try:
        data = await request.json()
        user_query = data.get("query")
        if not user_query:
            return web.json_response({"error": "Query parameter is required"}, status=400)

        result = get_text_response(user_query)
        return web.json_response({"result": result})

    except Exception as e:
        return web.json_response({"error": f"Error processing request: {e}"}, status=500)

async def image_analysis_gemini(request):
    try:
        data = await request.json()
        image_url = data.get("image_url")
        prompt = data.get("prompt")

        if not image_url:
            return web.json_response({"error": "Parameter 'image_url' is required"}, status=400)
        if not prompt:
            return web.json_response({"error": "Parameter 'prompt' is required"}, status=400)

        response = requests.get(image_url, stream=True)
        if response.status_code != 200:
            return web.json_response({"error": "Failed to download image from URL"}, status=400)

        with tempfile.NamedTemporaryFile(suffix=".jpg", delete=False) as temp_file:
            temp_file.write(response.content)
            image_path = temp_file.name

        result = analyze_image_with_prompt(image_path, prompt)

        os.remove(image_path)

        return web.json_response({"analysis": result})

    except requests.exceptions.MissingSchema:
        return web.json_response({"error": "Invalid URL format for 'image_url'"}, status=400)
    except Exception as e:
        return web.json_response({"error": f"Error processing image: {e}"}, status=500)

# Изменяем маршруты на POST
app.router.add_post("/", root)
app.router.add_post("/gpt-query-g4f", gpt_query_g4f)
app.router.add_post("/gpt-query-gemini", gpt_query_gemini)
app.router.add_post("/image-analysis-gemini", image_analysis_gemini)

if __name__ == "__main__":
    web.run_app(app, host="127.0.0.1", port=8000)
