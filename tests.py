import asyncio
import os
import pytest
from aiohttp.test_utils import TestClient, TestServer

from container import AppContainer
from aiohttp import MultipartWriter, FormData

pytest_plugins = ["aiohttp.pytest_plugin"]


@pytest.fixture
async def client(aiohttp_client) -> TestClient:
    container = AppContainer()
    server = container.async_server()
    return await aiohttp_client(server.app)


class TestAPI:
    # Проверяет, что сервер работает и возвращает ожидаемое сообщение на запрос по корневому пути
    async def test_root(self, client):
        resp = await client.post("/")
        assert resp.status == 200
        json_data = await resp.json()
        assert json_data["message"] == "Async aiohttp server is running"

    # Проверяет, что запрос с пустым телом для GPT-4 (g4f) возвращает ошибку с нужным сообщением
    async def test_empty_gpt_query_g4f(self, client):
        resp = await client.post("/gpt-query-g4f", json={})
        assert resp.status == 400
        json_data = await resp.json()
        assert json_data["error"] == "Query parameter is required"

    # Проверяет, что запрос с пустым телом для GPT-3 Gemini возвращает ошибку с нужным сообщением
    async def test_empty_gpt_query_gemini(self, client):
        resp = await client.post("/gpt-query-gemini", json={})
        assert resp.status == 400
        json_data = await resp.json()
        assert json_data["error"] == "Query parameter is required"

    # Проверяет, что запрос без параметра "flower" для /get_gz возвращает ошибку
    async def test_empty_get_gz(self, client):
        resp = await client.post("/get_gz", json={})
        assert resp.status == 400
        json_data = await resp.json()
        assert json_data["error"] == "Query parameter is required"

    # Проверяет, что запрос с пустым значением параметра "image_url" для /image-analysis-gemini возвращает ошибку
    async def test_invalid_image_analysis_gemini(self, client):
        resp = await client.post("/image-analysis-gemini", json={"image_url": ""})
        assert resp.status == 400
        json_data = await resp.json()
        assert json_data["error"] == "Parameter 'image_url' is required"

    # Проверяет, что отправка запроса на анализ изображения без файла возвращает ошибку
    async def test_image_analysis_file_no_file(self, client):
        with MultipartWriter("form-data") as mpwriter:
            pass

        resp = await client.post("/image-analysis-file", data=mpwriter)
        assert resp.status == 400
        json_data = await resp.json()
        assert json_data["error"] == "Parameter 'file' is required"

    # Проверяет, что корректный запрос с параметром "flower" для /get_gz возвращает правильный ответ с полем "hz"
    async def test_valid_get_gz(self, client):
        valid_payload = {"flower": "Роза"}
        resp = await client.post("/get_gz", json=valid_payload)
        assert resp.status == 200, "Expected 200 OK for valid get_gz request"
        json_data = await resp.json()
        assert "hz" in json_data, "Response should contain 'hz' field"
        assert isinstance(json_data["hz"], int), "Field 'hz' should be an integer"

    # Проверяет, что запрос без параметра "flower" для /get_gz возвращает ошибку
    async def test_invalid_get_gz_no_flower(self, client):
        invalid_payload = {}
        resp = await client.post("/get_gz", json=invalid_payload)
        assert resp.status == 400, "Expected 400 Bad Request for missing flower parameter"
        json_data = await resp.json()
        assert json_data["error"] == "Query parameter is required", "Unexpected error message in response"

    # Проверяет, что корректный запрос для GPT-4 (g4f) возвращает правильный результат
    async def test_valid_gpt_query_g4f(self, client):
        valid_payload = {"query": "Test query"}
        resp = await client.post("/gpt-query-g4f", json=valid_payload)
        assert resp.status == 200, "Expected 200 OK for valid gpt_query_g4f request"
        json_data = await resp.json()
        assert "result" in json_data, "Response should contain 'result' field"
        assert isinstance(json_data["result"], str), "Field 'result' should be a string"
        assert json_data["result"].strip(), "Field 'result' should not be empty"

    # Проверяет, что корректный запрос на анализ изображения возвращает ожидаемый результат (например, "Бархатцы")
    async def test_image_analysis_file(self, client):
        current_dir = os.path.dirname(os.path.abspath(__file__))
        test_image_path = os.path.join(current_dir, "Test_images", "Marigold.jpg")
        assert os.path.isfile(test_image_path), f"Test image not found at {test_image_path}"

        expected_result = ['Бархатцы']

        with open(test_image_path, "rb") as image_file:
            form_data = FormData()
            form_data.add_field("file", image_file, filename="test_image.jpg", content_type="image/jpeg")
            resp = await client.post("/image-analysis-file", data=form_data)

            assert resp.status == 200, "Expected 200 OK for valid image analysis request"
            json_data = await resp.json()
            assert isinstance(json_data, list), "Response should be a list of detected items"
            assert json_data == expected_result, f"Expected {expected_result}, but got {json_data}"

    # Проверяет, что попытка сохранить изображение без файла возвращает ошибку с нужным сообщением
    async def test_save_image_without_file(self, client):
        with MultipartWriter("form-data") as mpwriter:
            mpwriter.append("test_image", {"Content-Disposition": 'form-data; name="identifier"'})
            resp = await client.post("/save-image-with-id", data=mpwriter)

        assert resp.status == 400, "Expected 400 Bad Request for missing file"
        json_data = await resp.json()
        assert json_data["error"] == "Parameter 'file' is required"

    # Проверяет, что попытка сохранить изображение с пустым идентификатором возвращает ошибку
    async def test_save_image_with_empty_identifier(self, client):
        with MultipartWriter("form-data") as mpwriter:
            mpwriter.append(b"fake_image_data",
                            {"Content-Disposition": 'form-data; name="file"; filename="test_image.jpg"',
                             "Content-Type": "image/jpeg"})
            mpwriter.append("", {"Content-Disposition": 'form-data; name="identifier"'})
            resp = await client.post("/save-image-with-id", data=mpwriter)

        assert resp.status == 400, "Expected 400 Bad Request for empty identifier"
        json_data = await resp.json()
        assert json_data["error"] == "Identifier cannot be empty"

    # Проверяет успешную загрузку изображения с правильным идентификатором и файл
    async def test_save_image_success(self, client):
        with MultipartWriter("form-data") as mpwriter:
            mpwriter.append(b"fake_image_data",
                            {"Content-Disposition": 'form-data; name="file"; filename="test_image.jpg"',
                             "Content-Type": "image/jpeg"})
            mpwriter.append("test_image", {"Content-Disposition": 'form-data; name="identifier"'})
            resp = await client.post("/save-image-with-id", data=mpwriter)

        assert resp.status == 200, "Expected 200 OK for successful image upload"
        json_data = await resp.json()
        assert "message" in json_data
        assert "File saved successfully" in json_data["message"]

    # Проверяет, что запрос для получения несуществующего изображения возвращает ошибку 404
    async def test_get_non_existing_image(self, client):
        payload = {"identifier": "non_existing_image"}
        resp = await client.post("/get-image-by-id", json=payload)

        assert resp.status == 404, "Expected 404 Not Found for missing image"
        json_data = await resp.json()
        assert json_data["error"] == "Image with identifier 'non_existing_image' not found"

    # Проверяет успешное сохранение и получение изображения по его идентификатору
    async def test_get_image_success(self, client):
        with MultipartWriter("form-data") as mpwriter:
            mpwriter.append(b"fake_image_data",
                            {"Content-Disposition": 'form-data; name="file"; filename="test_image.jpg"',
                             "Content-Type": "image/jpeg"})
            mpwriter.append("test_image", {"Content-Disposition": 'form-data; name="identifier"'})
            save_resp = await client.post("/save-image-with-id", data=mpwriter)

        assert save_resp.status == 200, "Expected 200 OK for image upload"

        payload = {"identifier": "test_image"}
        resp = await client.post("/get-image-by-id", json=payload)

        assert resp.status == 200, "Expected 200 OK for successful image retrieval"
        assert resp.content_type == "image/jpeg", "Expected image/jpeg content type"

    # Тест на асинхронное сохранение и получение изображений
    async def test_async_save_and_retrieve_images(self, client):
        async def upload_image(identifier, file_content):
            with MultipartWriter("form-data") as mpwriter:
                mpwriter.append(file_content, {
                    "Content-Disposition": f'form-data; name="file"; filename="{identifier}.jpg"',
                    "Content-Type": "image/jpeg"
                })
                mpwriter.append(identifier, {"Content-Disposition": 'form-data; name="identifier"'})
                resp = await client.post("/save-image-with-id", data=mpwriter)
                assert resp.status == 200, f"Upload failed for {identifier}"

        async def get_image(identifier):
            payload = {"identifier": identifier}
            resp = await client.post("/get-image-by-id", json=payload)
            assert resp.status == 200, f"Retrieval failed for {identifier}"
            assert resp.content_type == "image/jpeg"

        await asyncio.gather(
            upload_image("image1", b"fake_image_data1"),
            upload_image("image2", b"fake_image_data2"))

        await asyncio.gather(get_image("image1"), get_image("image2"))

    # Тест на одновременный доступ к одному и тому же изображению
    # Одновременные запросы на получение одного и того же файла
    async def test_simultaneous_image_access(self, client):
        with MultipartWriter("form-data") as mpwriter:
            mpwriter.append(b"fake_image_data", {
                "Content-Disposition": 'form-data; name="file"; filename="test_image.jpg"',
                "Content-Type": "image/jpeg"})
            mpwriter.append("shared_image", {"Content-Disposition": 'form-data; name="identifier"'})
            save_resp = await client.post("/save-image-with-id", data=mpwriter)
            assert save_resp.status == 200, "Image upload failed"

        async def get_shared_image():
            payload = {"identifier": "shared_image"}
            resp = await client.post("/get-image-by-id", json=payload)
            assert resp.status == 200, "Failed to retrieve shared image"
            assert resp.content_type == "image/jpeg"

        await asyncio.gather(get_shared_image(), get_shared_image())
