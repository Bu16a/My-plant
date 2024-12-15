import schedule
import time
from plant_scheduler import PlantScheduler


class Scheduler:
    def __init__(self, plant_watering_scheduler: PlantScheduler):
        self.plant_watering_scheduler = plant_watering_scheduler

    def start(self):
        for event in self.plant_watering_scheduler.event_messages.keys():
            schedule.every(5).minutes.do(self.plant_watering_scheduler.check_and_send_notifications, event=event)

        print("Планировщик запущен. Ожидание заданий...")

        while True:
            schedule.run_pending()
            time.sleep(1)
