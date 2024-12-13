import pytest
from unittest.mock import AsyncMock, patch
from aiohttp.test_utils import TestClient
from container import AppContainer
from aiohttp import FormData


@pytest.fixture
async def client(aiohttp_client) -> TestClient:
    container = AppContainer()
    server = container.async_server()
    return await aiohttp_client(server.app)


class TestMockedAI:
    # Проверяет правильную обработку запроса к GPT-4 с использованием подмены результата
    @pytest.mark.asyncio
    @patch("ai_handlers.AIModelHandler.get_gpt_response_g4f", new_callable=AsyncMock)
    async def test_mocked_gpt_query_g4f(self, mock_gpt_response, client):
        mock_gpt_response.return_value = "Mocked Response"
        payload = {"query": "Test query"}
        test_client = await client
        resp = await test_client.post("/gpt-query-g4f", json=payload)
        assert resp.status == 200
        json_data = await resp.json()
        assert json_data["result"] == "Mocked Response"

    # Имитация загрузки изображения и анализа возвращает "Mocked Image Analysis"
    @pytest.mark.asyncio
    @patch("requests.get")
    @patch("ai_handlers.AIModelHandler.analyze_image_with_prompt")
    async def test_mocked_image_analysis(self, mock_analyze_image, mock_requests_get, client):
        mock_requests_get.return_value.status_code = 200
        mock_requests_get.return_value.content = b"fake_image_data"

        mock_analyze_image.return_value = "Mocked Image Analysis"
        payload = {"image_url": "http://example.com/test.jpg"}

        test_client = await client
        resp = await test_client.post("/image-analysis-gemini", json=payload)
        assert resp.status == 200
        json_data = await resp.json()
        assert json_data["analysis"] == "Mocked Image Analysis"

    # Проверяет анализ изображения, отправленного как файл, с использованием подмены результата
    @pytest.mark.asyncio
    @patch("ai_handlers.AIModelHandler.analyze_image_with_prompt")
    async def test_mocked_image_file_analysis(self, mock_analyze_image, client):
        mock_analyze_image.return_value = "Mocked File Analysis"

        form_data = FormData()
        form_data.add_field(
            "file",
            b"fake_image_data",
            filename="test_image.jpg",
            content_type="image/jpeg")

        test_client = await client
        resp = await test_client.post("/image-analysis-file", data=form_data)

        assert resp.status == 200
        json_data = await resp.json()
        assert json_data == ["Mocked File Analysis"]
