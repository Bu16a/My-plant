from aiohttp import web
from handlers import get_gpt_response
from urllib.parse import quote  # Импорт для кодирования строки
import g4f  # Импорт моделей для обработки модели по умолчанию

app = web.Application()

async def root(request):
    return web.json_response({"message": "Async aiohttp server is running"})

async def gpt_query(request):
    user_query = request.query.get("query")
    if not user_query:
        return web.json_response({"error": "Query parameter is required"}, status=400)

    # Кодирование строки запроса
    encoded_query = quote(user_query)

    # Получаем модель из запроса, если она указана
    model_name = request.query.get("model")
    model = getattr(g4f.models, model_name, g4f.models.default) if model_name else g4f.models.default

    # Запрос к нейросети с использованием указанной или дефолтной модели
    result = await get_gpt_response(encoded_query, model=model)
    return web.json_response({"result": result})

app.router.add_get("/", root)
app.router.add_get("/gpt-query", gpt_query)

if __name__ == "__main__":
    web.run_app(app, host="127.0.0.1", port=8000)
