import os
import pytest
from aiohttp.test_utils import TestClient, TestServer
from server import AsyncServer
from aiohttp import MultipartWriter, FormData

pytest_plugins = ["aiohttp.pytest_plugin"]


@pytest.fixture
async def client(aiohttp_client) -> TestClient:
    server = AsyncServer()
    return await aiohttp_client(server.app)


class TestAPI:
    async def test_root(self, client):
        resp = await client.post("/")
        assert resp.status == 200
        json_data = await resp.json()
        assert json_data["message"] == "Async aiohttp server is running"

    async def test_empty_gpt_query_g4f(self, client):
        resp = await client.post("/gpt-query-g4f", json={})
        assert resp.status == 400
        json_data = await resp.json()
        assert json_data["error"] == "Query parameter is required"

    async def test_empty_gpt_query_gemini(self, client):
        resp = await client.post("/gpt-query-gemini", json={})
        assert resp.status == 400
        json_data = await resp.json()
        assert json_data["error"] == "Query parameter is required"

    async def test_empty_get_gz(self, client):
        resp = await client.post("/get_gz", json={})
        assert resp.status == 400
        json_data = await resp.json()
        assert json_data["error"] == "Query parameter is required"

    async def test_invalid_image_analysis_gemini(self, client):
        resp = await client.post("/image-analysis-gemini", json={"image_url": ""})
        assert resp.status == 400
        json_data = await resp.json()
        assert json_data["error"] == "Parameter 'image_url' is required"

    async def test_image_analysis_file_no_file(self, client):
        with MultipartWriter("form-data") as mpwriter:
            pass

        resp = await client.post("/image-analysis-file", data=mpwriter)
        assert resp.status == 400
        json_data = await resp.json()
        assert json_data["error"] == "Parameter 'file' is required"

    async def test_valid_get_gz(self, client):
        valid_payload = {"flower": "Роза"}
        resp = await client.post("/get_gz", json=valid_payload)
        assert resp.status == 200, "Expected 200 OK for valid get_gz request"
        json_data = await resp.json()
        assert "hz" in json_data, "Response should contain 'hz' field"
        assert isinstance(json_data["hz"], int), "Field 'hz' should be an integer"

    async def test_invalid_get_gz_no_flower(self, client):
        invalid_payload = {}
        resp = await client.post("/get_gz", json=invalid_payload)
        assert resp.status == 400, "Expected 400 Bad Request for missing flower parameter"
        json_data = await resp.json()
        assert json_data["error"] == "Query parameter is required", "Unexpected error message in response"

    async def test_valid_gpt_query_g4f(self, client):
        valid_payload = {"query": "Test query"}
        resp = await client.post("/gpt-query-g4f", json=valid_payload)
        assert resp.status == 200, "Expected 200 OK for valid gpt_query_g4f request"
        json_data = await resp.json()
        assert "result" in json_data, "Response should contain 'result' field"
        assert isinstance(json_data["result"], str), "Field 'result' should be a string"
        assert json_data["result"].strip(), "Field 'result' should not be empty"

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
