import tkinter as tk
import numpy as np


class Window(tk.Frame):
    def __init__(self, parent):
        super().__init__()
        self.__parent = parent
        self.__ui_init()

    def __ui_init(self):
        self.__parent.title("Plot")
        self.__functions = [
            ('x^2', np.vectorize(lambda x: x ** 2)),
            ('x^3', np.vectorize(lambda x: x ** 3)),
            ('sin(x)', np.sin),
            ('cos(x)', np.cos),
            ('e^x', np.exp)
        ]
        self.__canvas = tk.Canvas(width=200, height=200)
        self.__canvas_height, self.__canvas_width = self.__canvas.winfo_height(), self.__canvas.winfo_width()
        self.__canvas.pack(fill=tk.BOTH, expand=True)
        self.__canvas.delete(tk.ALL)
        self.__parent.bind('<Configure>', self.__on_resize)

        self.__list = tk.Listbox(selectmode=tk.SINGLE, height=5)
        for i, _ in self.__functions:
            self.__list.insert(tk.END, i)
        self.__list.pack(side=tk.BOTTOM)
        self.__list.bind('<Double-1>', self.__on_select_function)

        self.__begin = tk.Entry(width=15)
        self.__begin.insert(0, '-10')
        self.__begin_label = tk.Label(text='from')
        self.__begin_label.pack(side=tk.LEFT)
        self.__begin.pack(side=tk.LEFT)
        self.__end = tk.Entry(width=15)
        self.__end.insert(0, '10')
        self.__end_label = tk.Label(text='to')
        self.__end.pack(side=tk.RIGHT)
        self.__end_label.pack(side=tk.RIGHT)

        self.__current_function = self.__functions[0][1]
        self.__range = (float(self.__begin.get()), float(self.__end.get()))
        self.__redraw()

    def __on_select_function(self, event):
        self.__current_function = self.__functions[self.__list.curselection()[0]][1]
        self.__create_plot()

    def __create_plot(self):
        self.__range = (float(self.__begin.get()), float(self.__end.get()))
        self.__begin.delete(0, tk.END)
        self.__end.delete(0, tk.END)
        self.__begin.insert(0, str(self.__range[0]))
        self.__end.insert(0, str(self.__range[1]))
        self.__redraw()

    def __redraw(self):
        self.__canvas.delete(tk.ALL)
        count_points = self.__canvas.winfo_width() - 2
        if count_points < 1:
            return
        self.__canvas.create_rectangle(1, 1, self.__canvas.winfo_width()-2, self.__canvas.winfo_height()-2, outline='blue')
        xs = np.linspace(self.__range[0], self.__range[1], count_points)
        ys = self.__current_function(xs)
        min_value, max_value = ys.min(), ys.max()
        ys = (ys - min_value) / (max_value - min_value) * (self.__canvas.winfo_height() - 3)
        ys = self.__canvas.winfo_height() - ys - 3
        for i in range(count_points - 1):
            self.__canvas.create_line(i+1, ys[i], i+2, ys[i+1])

    def __on_resize(self, event):
        ch, cw = self.__canvas.winfo_height(), self.__canvas.winfo_width()
        self.__canvas_height, self.__canvas_width = ch, cw
        self.__redraw()


def main():
    parent = tk.Tk()
    window = Window(parent)
    window.mainloop()


if __name__ == '__main__':
    main()
