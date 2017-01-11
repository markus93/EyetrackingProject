# Calculating average time and standard deviation.
# Also generating specific heat maps over all participants data.

import os
import pandas  # Data frame
import numpy  # numeric calculation
import matplotlib.pyplot as plt  # Plotting


def heat_map2(file_name, log_files):
    # Get collective heat map over many files

    save_path = "../final_data/"

    if not os.path.exists(save_path):
        os.makedirs(save_path)

    X = []
    Y = []

    for fname in log_files:
        data = pandas.read_csv(fname, delimiter=',', header=None, index_col=0, na_values='NaN', decimal=".")
        data.sort_index(inplace=True)
        x_list = data[1].tolist()
        y_list = data[2].tolist()

        x_list, y_list = remove_negative_values(x_list[:], y_list[:])

        X = X + x_list
        Y = Y + y_list

    heatmap, xedges, yedges = np.histogram2d(X, Y, bins=50)
    extent = [0, xedges[-1], 0, yedges[-1]]

    plt.clf()
    plt.imshow(heatmap.T, extent=extent, origin='lower')
    plt.savefig(save_path + file_name)
    #plt.show()


def remove_negative_values(l1, l2):

    max_len = len(l1)

    for i in range(1, len(l1)):
        if l1[max_len-i] < 0 or l2[max_len-i] < 0:  # get index of element from end or indexing will get messed up
            l1.pop(max_len-i)
            l2.pop(max_len-i)

    return l1, l2


def get_times_and_logs(path, file_name):

    times = []
    log_files = []

    with open(path + file_name, 'r') as file:
        text = file.read()  # Read the text of the file into memory.

    text = text.split("\n")
    text = filter(None, text)

    for line in text:
        line_split = line.split(" ")
        time = int(line_split[0])
        times.append(time)
        # get log file path
        file_path = " ".join(line_split[1:])
        path_split = file_path.split(", ")
        time_of_experiment = path_split[1].split(")")[0]
        path = "../log/" + path_split[0][1:] + "/" + file_name[:-4] + " " + time_of_experiment + ".txt"
        log_files.append(path)

    return times, log_files


def get_times_and_logs2(path, file_name):
    times = [[],[],[],[],[]]
    log_files = [[],[],[],[],[]]

    with open(path + file_name, 'r') as file:
        text = file.read()  # Read the text of the file into memory.

    text = text.split("\n")
    text = filter(None, text)

    subject_nr = 1
    i = 0

    for line in text:
        line_split = line.split(" ")
        time = int(line_split[0])
        # get log file path
        file_path = " ".join(line_split[1:])
        path_split = file_path.split(", ")
        dir_name = path_split[0][1:]
        sub_nr = int(dir_name[1])  # number of the subject
        time_of_experiment = path_split[1].split(")")[0]
        path = "../log/" + dir_name + "/" + file_name[:-4] + " " + time_of_experiment + ".txt"

        if sub_nr != subject_nr:
            subject_nr = sub_nr
            i = 0

        log_files[i].append(path)
        times[i].append(time)
        i += 1

    return times, log_files


def calculate_avg_and_sd(times):

    avg = sum(times)/float(len(times))
    sd = numpy.std(times)

    return avg, sd


def data_for1to4():
    # read file

    path = "../info_ver2/"
    file_name1 = "RB_reading (initial - god).txt"
    file_name2 = "RB_reading (aftereffect - god).txt"
    file_name3 = "RB_reading (initial - north).txt"
    file_name4 = "RB_reading (aftereffect - north).txt"

    file_names = [file_name1, file_name2, file_name3, file_name4]

    # get times and file paths out of text. Last in order to generate heat map over all trials
    for file_name in file_names:
        times, log_files = get_times_and_logs(path, file_name)
        print(times)
        avg, sd = calculate_avg_and_sd(times)
        print(file_name + ":")
        print("Average is: " + str(round(avg, 3)))
        print("SD is: " + str(round(sd, 3)))

        heat_map2(file_name[:-4], log_files)


def data_for7to10():

    path = "../info_ver2/"
    file_name1 = "RB_reading (change to initial - smart).txt"
    file_name2 = "RB_reading (change word - salt).txt"

    file_names = [file_name1, file_name2]

    for file_name in file_names:

        times, log_files = get_times_and_logs2(path, file_name)

        for i in range(0, len(times)):
            avg, sd = calculate_avg_and_sd(times[i])
            print(file_name + " (reading " + str(i + 1) + ")" + ":")
            print("Average is: " + str(round(avg, 3)))
            print("SD is: " + str(round(sd, 3)))

            heat_map2(file_name[:-4] + " reading " + str(i + 1), log_files[i])


if __name__ == '__main__':

    data_for1to4()
    data_for7to10()




