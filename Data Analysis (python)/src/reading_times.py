# Read all files in folder and calculate time spent
import os


# Calculates spent time on log file
def get_time_from_file(file_name):
    with open(file_name, 'r') as file:
        text = file.read()  # Read the text of the file into memory.
    my_list = text.splitlines()
    start_time = my_list[0].split("__")[0]
    end_time = my_list[-1].split("__")[0]
    start_time = get_seconds(start_time, '_')
    end_time = get_seconds(end_time, '_')

    return end_time - start_time


def get_seconds(time, split_char):
    return sum(x * int(t) for x, t in zip([3600, 60, 1], time.split(split_char)))


def write_to_dir(file_name, level_name, time_spent, start_time):

    with open(file_name, "a") as f:
        f.write("%d (%s, %s)\n" % (time_spent, level_name, start_time))

if __name__ == '__main__':

    dirs = [x for x in os.listdir("../log")]

    for dir_name in dirs:
        files = [x for x in os.listdir("../log/" + dir_name)]
        for f in files:
            split_f = f.split()
            level_name = " ".join(split_f[:-1])
            start_time = (split_f[-1]).split(".txt")[0]
            file_name = dir_name + "/" + f

            time_spent = get_time_from_file("../log/" + file_name)

            write_to_dir("../info_ver1/" + dir_name + ".txt", level_name, time_spent, start_time)
            write_to_dir("../info_ver2/" + level_name + ".txt", dir_name, time_spent, start_time)



