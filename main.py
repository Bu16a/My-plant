from firebase_client import FirebaseClient
from notification_service import NotificationService
from plant_scheduler import PlantScheduler
from scheduler import Scheduler


def main():
    firebase_client = FirebaseClient(
        credential_path="uplant-36fdf-firebase-adminsdk-5pkef-9fd4996e6f.json",
        database_url="https://uplant-36fdf-default-rtdb.europe-west1.firebasedatabase.app/"
    )
    notification_service = NotificationService()
    plant_scheduler = PlantScheduler(firebase_client, notification_service)
    scheduler = Scheduler(plant_scheduler)

    scheduler.start()

if __name__ == "__main__":
    main()
