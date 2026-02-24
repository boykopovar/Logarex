def factorial(n):
    if n == 0:
        return 1
    else:
        return n * factorial(n - 1)

def is_prime(num):
    if num < 2:
        return False
    for i in range(2, int(num ** 0.5) + 1):
        if num % i == 0:
            return False
    return True

def max_in_list(numbers):
    if not numbers:
        return None
    max_val = numbers[0]
    for val in numbers:
        if val > max_val:
            max_val = val
    return max_val

def main():
    while True:
        print("\nВыберите операцию:")
        print("1. Вычислить факториал")
        print("2. Проверить, простое ли число")
        print("3. Найти максимум в списке")
        print("4. Выход")
        choice = input("Введите номер пункта: ")
        if choice == '1':
            try:
                n = int(input("Введите неотрицательное целое число: "))
                if n < 0:
                    print("Факториал определён только для неотрицательных чисел.")
                else:
                    result = factorial(n)
                    print(f"Факториал числа {n} равен {result}")
            except ValueError:
                print("Ошибка: введите целое число.")
        elif choice == '2':
            try:
                num = int(input("Введите целое число: "))
                if is_prime(num):
                    print(f"{num} — простое число.")
                else:
                    print(f"{num} — составное число.")
            except ValueError:
                print("Ошибка: введите целое число.")
        elif choice == '3':
            try:
                numbers = list(map(int, input("Введите числа через пробел: ").split()))
                if not numbers:
                    print("Список пуст.")
                else:
                    max_val = max_in_list(numbers)
                    print(f"Максимальное число в списке: {max_val}")
            except ValueError:
                print("Ошибка: введите целые числа, разделённые пробелами.")
        elif choice == '4':
            print("Выход из программы.")
            break
        else:
            print("Неверный пункт меню. Попробуйте снова.")

if __name__ == "__main__":
    main()
