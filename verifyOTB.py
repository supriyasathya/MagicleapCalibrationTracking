# The following code is used to watch a video stream, detect Aruco markers, and use
# a set of markers to determine the posture of the camera in relation to the plane
# of markers.
# Assumes that all markers are on the same plane, for example on the same piece of paper
# Requires camera calibration (see the rest of the project for example calibration)


import numpy as np
import cv2
import cv2.aruco as aruco
from arucocalibclass import arucocalibclass
from charucocalibclass import charucocalibclass
from arucodetectRSML import startTracking

import csv
import numpy as np
import pyrealsense2
import cv2
import cv2.aruco as aruco
import os
import pickle
import pyrealsense2 as rs
import matplotlib.pyplot as plt
import struct
from scipy.spatial.transform import Rotation as R
from numpy.linalg import inv
import socket
from csv import writer


#names of csv files to save the ransformation matrices
file_name_TRSML = '/Users/supriya/Documents/Codes/Charuco_camera_calib/CharucoCalibration/TRSML.csv'
file_name_TM1ML = '//Users/supriya/Documents/Codes/Charuco_camera_calib/CharucoCalibration/TM1ML.csv'
file_name_TM1RS = '/Users/supriya/Documents/Codes/Charuco_camera_calib/CharucoCalibration/TM1RS.csv'
file_name_TM2ML = '/Users/supriya/Documents/Codes/Charuco_camera_calib/CharucoCalibration/TM2ML.csv'
file_name_TM2RS = '//Users/supriya/Documents/Codes/Charuco_camera_calib/CharucoCalibration/TM2RS.csv'
file_name_TM2RS_Matrix = '/Users/supriya/Documents/Codes/Charuco_camera_calib/CharucoCalibration/TM2RSMatrix.csv'
with open(file_name_TRSML, 'a+', newline='') as write_obj:
    csv_writer = writer(write_obj)
    csv_writer.writerow('Nov6 - Char-MOV--- next trial')
with open(file_name_TM1ML, 'a+', newline='') as write_obj:
    csv_writer = writer(write_obj)
    csv_writer.writerow('Nov6 - Char-MOV--- next trial')
with open(file_name_TM1RS, 'a+', newline='') as write_obj:
    csv_writer = writer(write_obj)
    csv_writer.writerow('Nov6 - Char-MOV--- next trial')
with open(file_name_TM2ML, 'a+', newline='') as write_obj:
    csv_writer = writer(write_obj)
    csv_writer.writerow('Nov6- Char-MOV--- next trial')
with open(file_name_TM2RS, 'a+', newline='') as write_obj:
    csv_writer = writer(write_obj)
    csv_writer.writerow('Nov6 - Char-MOV--- next trial')
with open(file_name_TM2RS_Matrix, 'a+', newline='') as write_obj:
    csv_writer = writer(write_obj)
    csv_writer.writerow('Nov6 - Char-MOV--- next trial')

def createSocket(socket_connect):
    if socket_connect == 1:
        s = socket.socket()
        print("Socket successfully created")
        # reserve a port on your computer - for example 2020 - but it can be anything. The same port number must be given in the Unity scene of the App deployed on to Magicleap.
        port = 2020
        s.bind(('', port))
        print("socket binded to %s" % (port))

        # put the socket into listening mode
        s.listen(5)
        print("socket is listening")
        #accept connection to server
        c, addr = s.accept()
        print('got connection from ', addr)
    return c, addr



def verifyT_RS_ML(marker_type, socket_connect,  n_sq_y, n_sq_x, squareLen, markerLen, aruco_dict, T_M2_RS, c):
    print("came to verify calib part")
    #T_M2_ML is the pose of marker 2 in Magicleap space
    T_M2_ML = np.zeros([4, 4], np.float32)
    n_avg = 0
    N_samples = 2 #number of samples of T_M2_ML to be averaged out and taken.
    while (True):
        # get T_M2_ML from MagicLeap
        dataRecv = c.recv(64)
        MLArr = np.frombuffer(dataRecv, dtype=np.float32)
        print("verify MLArr", MLArr)
        if MLArr[3] != 0:
            T_M2_ML += MLArr.reshape(4, 4)
            # following 3 lines is for saving T_M2_ML in csv file
            with open(file_name_TM2ML, 'a+', newline='') as write_obj:
                csv_writer = writer(write_obj)
                # T_M2_ML
                csv_writer.writerow([T_M2_ML])
            print("T_M2_ML", T_M2_ML)
            n_avg += 1
            if n_avg == N_samples:
                T_M2_ML = T_M2_ML / N_samples
                print("T_M2_ML average", T_M2_ML)
                # following 3 lines is for saving T_M2_ML_average in csv file
                with open(file_name_TM2ML, 'a+', newline='') as write_obj:
                    csv_writer = writer(write_obj)
                    # T_M2_ML
                    csv_writer.writerow([T_M2_ML])
                # compute T_RS_ML2 (transformation from Realsense space to Magicleap space
                T_RS_ML2 = np.matmul(T_M2_ML, inv(T_M2_RS))
                # call startTracking function to start tracking in Realsense space and then stream the transformed pose to Magicleap space, given the transformation matrix T_RS_ML2 just computed.
                startTracking(marker_type, T_RS_ML2,  n_sq_y, n_sq_x, squareLen, markerLen, aruco_dict, socket_connect, c)
                #break

def main():
    # set flag based on which marker we want to detect
    #Marker type: 1 - Charucoboard,  else - single aruco
    marker_type = 1
    socket_connect = True
    kalman_flag = False
    calibStep1Flag = False
    if marker_type == 1: #charuco board
        squareLen = 0.03710#02382#0.033#0.025 # 0.036
        markerLen = 0.02250#0.01456#0.02  # 0.0215
        n_sq_y = 7
        n_sq_x = 5
        aruco_dict = aruco.Dictionary_get(aruco.DICT_6X6_250)
    else: # single aruco
        squareLen = None
        markerLen = 0.061
        n_sq_y = None
        n_sq_x = None
        aruco_dict = aruco.Dictionary_get(aruco.DICT_4X4_250)

    if socket_connect == True:
        # call function to create network socket and connect with the server initiated by MagicLeap
        c, addr = createSocket(socket_connect)
        # T_M2_RS that was computed in step 2 of one-time calibration is input here. Replace this array with updated values if the marker 2 attached to Realsense is changes/moved
        T_M2_RS = np.array([[-1.00108384 , 0.00161849  ,0.01639776 , 0.04185618, -0.00199964, -1.0010518 , -0.0237722 , -0.02449649,
            0.01613709 ,-0.02392271,  0.99971235,  -0.00614097,  0.      ,    0.     ,     0.      ,    1.        ]])
        T_M2_RS = T_M2_RS.reshape(4, 4)
        
        # call the function to test the one-time calibration
        verifyT_RS_ML(marker_type, socket_connect,  n_sq_y, n_sq_x, squareLen, markerLen, aruco_dict, T_M2_RS, c)


    # When everything done, release the capture
    cv2.destroyAllWindows()
    s.close()

if __name__ == '__main__':
    main()
