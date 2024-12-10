from enum import StrEnum


class Prompts(StrEnum):
    image_flower_prompt = (
            "Выведи список растений, через запятую которые изображены на картинке, самые похожие на изображённые, постарайся" +
            " максимально уточнить название, т.е. к примеру не просто кактус, а клейстокактус штраусса." +
            " Выведи только названия через запятую. Пример ответа: Роза Бархатцы Пионы. Если на картинке " +
            "нет растений то вывести слово - Растений_нет. Ничего другого выводить не нужно, только как в примерах!")

    watering_schedule = ('Составь подробный план полива для растения роза. Укажи расписание полива на ' +
                         'месяц, выдай ответ в формате списка, дату у время полива')

    flower_instruction = (
                'Print the frequency of watering in hours for the specified plant, i.e. Answer by indicating' +
                ' only one number, number of hours. Example answer: 4. That is. your answer must be only a ' +
                'number without letters, only one number. Execute the indicated commands for: ')
