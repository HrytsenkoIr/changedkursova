Файл містить список основних принципів програмування мого проєкту. Для прикладу наведено опис того, як ці принципи реалізовані в модулі категорій товарів.

Single Responsibility Principle
Кожен клас відповідає за свою конкретну задачу. Контролер обробляє запити, репозиторій працює з БД, а модель описує дані. Наприклад клас CategoryRepository виконує лише операції з базою даних.
Посилання: ./Repositories/CategoryRepository.cs

Dependency Inversion Principle
Контролер залежить від інтерфейсу, а не від конкретного класу репозиторію. Наприклад конструктор контролера приймає інтерфейс ICategoryRepository.
Посилання: ./Controllers/CategoryController.cs#L15

Interface Segregation Principle
Створено окремий вузький інтерфейс суто для роботи з категоріями. Наприклад інтерфейс ICategoryRepository з методами CRUD для категорій.
Посилання: ./Repositories/Interfaces/ICategoryRepository.cs

DRY
Логіка перевірки наявності товарів у категорії винесена в окремий метод для повторного використання. Наприклад метод HasProductsAsync.
Посилання: ./Repositories/CategoryRepository.cs#L95

KISS
Методи контролера максимально прості та не містять зайвої логіки перетворення даних. Напркилад метод Index у контролері.
Посилання: ./Controllers/CategoryController.cs#L21-L25

Інкапсуляція
Внутрішні залежності класів приховані за допомогою модифікаторів доступу. Наприклад поля _categoryRepository та _context позначені як private readonly.
Посилання: ./Controllers/CategoryController.cs#L13

Поліморфізм
Використання інтерфейсу репозиторію та віртуальних властивостей у моделях. Наприклад властивість virtual ICollection<Product>.
Посилання: ./EFModels/Category.cs#L20

Розділення логіки
Проєкт чітко поділений на рівні: дизайн, контролери та доступ до даних.
Посилання: ./Controllers/
Посилання: ./EFModels/
Посилання: ./Views/
