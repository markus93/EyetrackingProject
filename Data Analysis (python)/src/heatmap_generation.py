# Used to generate heat map for log file (atm generates for all files in log directory

import pandas as pd  # Dataframe
import numpy as np  # numeric calculation
import matplotlib.pyplot as plt  # Plotting
import os


def heatmap(dirname, fname):
    path = '../log/' + dirname + "/"
    save_path = '../heatmaps/%s/' % dirname

    if not os.path.exists(save_path):
        os.makedirs(save_path)

    imgname = fname.split('.')[:-1]
    imgname = ".".join(imgname)
    data = pd.read_csv(path + fname, delimiter=',', header=None, index_col=0, na_values='NaN', decimal=".")
    data.sort_index(inplace=True)
    x_list = data[1].tolist()
    y_list = data[2].tolist()

    x_list, y_list = remove_negative_values(x_list[:], y_list[:])

    heatmap, xedges, yedges = np.histogram2d(x_list, y_list, bins=50)
    extent = [0, xedges[-1], 0, yedges[-1]]

    # Put data to plot and show/save plot
    plt.clf()
    plt.imshow(heatmap.T, extent=extent, origin='lower')
    plt.savefig(save_path + imgname)
    #plt.show()


def remove_negative_values(l1, l2):

    max_len = len(l1)

    for i in range(1, len(l1)):
        if l1[max_len-i] < 0 or l2[max_len-i] < 0:  # get index of element from end or indexing will get messed up
            l1.pop(max_len-i)
            l2.pop(max_len-i)

    return l1, l2

if __name__ == '__main__':

    # Get all directories from log folder
    dirs = [x for x in os.listdir("../log")]

    for dir_name in dirs:
        files = [x for x in os.listdir("../log/" + dir_name)]
        # Get all files from directory
        for f in files:
            print("%s, %s" % (dir_name, f))
            heatmap(dir_name, f)  # Generate heat map for every file separately
