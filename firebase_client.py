import firebase_admin
from firebase_admin import credentials, db
from typing import Any, Optional


class FirebaseClient:
    def __init__(self, credential_path: str, database_url: str):
        self.cred = credentials.Certificate(credential_path)
        firebase_admin.initialize_app(
            self.cred,
            {
                "databaseURL": database_url
            },
        )

    def get_object_from_db(self, path: str) -> Optional[Any]:
        try:
            ref = db.reference(path)
            data = ref.get()
            if data is None:
                print(f"Объект по пути '{path}' не найден.")
            return data
        except Exception as e:
            print(f"Ошибка при получении объекта: {e}")
            return None

    def update_object_in_db(self, path: str, data: dict) -> bool:
        try:
            ref = db.reference(path)
            ref.update(data)
            return True
        except Exception as e:
            print(f"Ошибка при обновлении объекта: {e}")
            return False
