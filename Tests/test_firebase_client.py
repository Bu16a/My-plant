import unittest
from unittest.mock import patch, MagicMock
from firebase_client import FirebaseClient


class TestFirebaseClient(unittest.TestCase):
    @patch('firebase_admin.initialize_app')
    def setUp(self, mock_initialize_app):
        self.firebase_client = FirebaseClient(
            credential_path="uplant-36fdf-firebase-adminsdk-5pkef-9fd4996e6f.json",
            database_url="https://example.firebaseio.com/"
        )

    @patch('firebase_admin.db.reference')
    def test_get_object_from_db_success(self, mock_db_ref):
        mock_ref = MagicMock()
        mock_ref.get.return_value = {"key": "value"}
        mock_db_ref.return_value = mock_ref

        result = self.firebase_client.get_object_from_db("test/path")
        self.assertEqual(result, {"key": "value"})
        mock_ref.get.assert_called_once()

    @patch('firebase_admin.db.reference')
    def test_get_object_from_db_not_found(self, mock_db_ref):
        mock_ref = MagicMock()
        mock_ref.get.return_value = None
        mock_db_ref.return_value = mock_ref

        result = self.firebase_client.get_object_from_db("test/path")
        self.assertIsNone(result)
        mock_ref.get.assert_called_once()

    @patch('firebase_admin.db.reference')
    def test_update_object_in_db_success(self, mock_db_ref):
        mock_ref = MagicMock()
        mock_ref.update.return_value = None
        mock_db_ref.return_value = mock_ref

        result = self.firebase_client.update_object_in_db("test/path", {"key": "value"})
        self.assertTrue(result)
        mock_ref.update.assert_called_once_with({"key": "value"})

    @patch('firebase_admin.db.reference')
    def test_update_object_in_db_failure(self, mock_db_ref):
        mock_ref = MagicMock()
        mock_ref.update.side_effect = Exception("Update failed")
        mock_db_ref.return_value = mock_ref

        result = self.firebase_client.update_object_in_db("test/path", {"key": "value"})
        self.assertFalse(result)
        mock_ref.update.assert_called_once_with({"key": "value"})


if __name__ == '__main__':
    unittest.main()
