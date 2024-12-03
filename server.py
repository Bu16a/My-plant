from aiohttp import web
from handlers import get_gpt_response_g4f, analyze_image_with_prompt, get_text_response
import g4f
import os
import requests
import tempfile
from prompts import Prompts
import re

app = web.Application()


async def root(request):
    return web.json_response({"message": "Async aiohttp server is running "})


async def gpt_query_g4f(request):
    try:
        data = await request.json()
        user_query = data.get("query", Prompts.watering_schedule)
        if not user_query:
            return web.json_response({"error": "Query parameter is required"}, status=400)

        model_name = data.get("model")
        model = getattr(g4f.models, model_name, g4f.models.default) if model_name else g4f.models.default

        result = await get_gpt_response_g4f(user_query, model=model)
        return web.json_response({"result": result.strip()})

    except Exception as e:
        return web.json_response({"error": f"Error processing request: {e}"}, status=500)


async def get_gz(request):
    try:
        data = await request.json()
        user_query = data.get("flower")
        user_query = Prompts.flower_instruction + user_query
        if not user_query:
            return web.json_response({"error": "Query parameter is required"}, status=400)
        result = get_text_response(user_query).replace('\n', '').strip()
        # result = await get_gpt_response_g4f(user_query, model=g4f.models.default)
        return web.json_response({"hz": int(result)})

    except Exception as e:
        return web.json_response({"error": f"Error processing request: {e}"}, status=500)


async def gpt_query_gemini(request):
    try:
        data = await request.json()
        user_query = data.get("query", Prompts.watering_schedule)
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
        prompt = data.get("prompt", Prompts.image_flower_prompt)

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


async def image_analysis_file(request):
    try:
        reader = await request.multipart()

        field = await reader.next()
        if field.name != "file":
            return web.json_response({"error": "Parameter 'file' is required"}, status=400)

        with tempfile.NamedTemporaryFile(suffix=".jpg", delete=False) as temp_file:
            while True:
                chunk = await field.read_chunk()
                if not chunk:
                    break
                temp_file.write(chunk)
            image_path = temp_file.name

        field = await reader.next()
        if field and field.name == "prompt":
            prompt = await field.text()
        else:
            prompt = Prompts.image_flower_prompt

        result = analyze_image_with_prompt(image_path, prompt).replace('\n', '').split(',')
        for i in range(len(result)):
            result[i] = re.sub(r'[.,\"#?!]', '', result[i]).strip()
        result = list(set(result))
        if 'Растений_нет' in result:
            result = []
        os.remove(image_path)

        return web.json_response(result)

    except Exception as e:
        return web.json_response({"error": f"Error processing image: {e}"}, status=500)


async def save_image_with_id(request):
    try:
        reader = await request.multipart()

        file_field = await reader.next()
        if file_field.name != "file":
            return web.json_response({"error": "Parameter 'file' is required"}, status=400)

        with tempfile.NamedTemporaryFile(delete=False) as temp_file:
            while True:
                chunk = await file_field.read_chunk()
                if not chunk:
                    break
                temp_file.write(chunk)
            temp_file_path = temp_file.name

        text_field = await reader.next()
        if text_field.name != "identifier":
            return web.json_response({"error": "Parameter 'identifier' is required"}, status=400)

        identifier = await text_field.text()
        if not identifier.strip():
            return web.json_response({"error": "Identifier cannot be empty"}, status=400)
        save_folder = "uploaded_images"
        os.makedirs(save_folder, exist_ok=True)
        save_path = os.path.join(save_folder, f"{identifier}.jpg")
        if os.path.exists(save_path):
            os.remove(save_path)
        os.rename(temp_file_path, save_path)

        return web.json_response({"message": f"File saved successfully as {save_path}"})

    except Exception as e:
        return web.json_response({"error": f"Error saving file: {e}"}, status=500)


async def get_image_by_id(request):
    try:
        data = await request.json()
        identifier = data.get("identifier")
        if not identifier:
            return web.json_response({"error": "Parameter 'identifier' is required"}, status=400)
        save_folder = "uploaded_images"
        image_path = os.path.join(save_folder, f"{identifier}.jpg")
        if not os.path.exists(image_path):
            return web.json_response({"error": f"Image with identifier '{identifier}' not found"}, status=404)
        return web.FileResponse(image_path)
    except Exception as e:
        return web.json_response({"error": f"Error retrieving image: {e}"}, status=500)


app.router.add_post("/image-analysis-file", image_analysis_file)
app.router.add_post("/", root)
app.router.add_post("/gpt-query-g4f", gpt_query_g4f)
app.router.add_post("/gpt-query-gemini", gpt_query_gemini)
app.router.add_post("/image-analysis-gemini", image_analysis_gemini)
app.router.add_post("/image-analysis-file", image_analysis_file)
app.router.add_post("/save-image-with-id", save_image_with_id)
app.router.add_post("/get-image-by-id", get_image_by_id)
app.router.add_post('/get_gz', get_gz)

if __name__ == "__main__":
    web.run_app(app, host="0.0.0.0", port=52)
