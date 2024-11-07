from aiohttp import web
from handlers import get_gpt_response_g4f, analyze_image_with_prompt, get_text_response
from urllib.parse import quote
import g4f
import os
import requests
import tempfile

app = web.Application()


async def root(request):
    return web.json_response({"message": "Async aiohttp server is running "})


async def gpt_query_g4f(request):
    user_query = request.query.get("query")
    if not user_query:
        return web.json_response({"error": "Query parameter is required"}, status=400)

    encoded_query = quote(user_query)
    model_name = request.query.get("model")
    model = getattr(g4f.models, model_name, g4f.models.default) if model_name else g4f.models.default

    result = await get_gpt_response_g4f(encoded_query, model=model)
    return web.json_response({"result": result})


async def gpt_query_gemini(request):
    user_query = request.query.get("query")
    if not user_query:
        return web.json_response({"error": "Query parameter is required"}, status=400)

    result = get_text_response(user_query)
    return web.json_response({"result": result})


async def image_analysis_gemini(request):
    try:
        image_url = request.query.get("image_url")
        prompt = request.query.get("prompt")

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


app.router.add_get("/", root)
app.router.add_get("/gpt-query-g4f", gpt_query_g4f)
app.router.add_get("/gpt-query-gemini", gpt_query_gemini)
app.router.add_get("/image-analysis-gemini", image_analysis_gemini)

if __name__ == "__main__":
    web.run_app(app, host="127.0.0.1", port=8000)
