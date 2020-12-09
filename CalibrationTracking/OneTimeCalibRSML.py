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
import time


# csv filenames to save the transformation matrices
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

# function to create network socket
def createSocket(socket_connect):
    if socket_connect == 1:
        s = socket.socket()
        print("Socket successfully created")
        # reserve a port on your computer - for example 2017 - but it can be anything
        port = 2020
        s.bind(('', port))
        print("socket binded to %s" % (port))

        # put the socket into listening mode
        s.listen(5)
        print("socket is listening")
        c, addr = s.accept() #connection with server accepted
        print('got connection from ', addr)
    return c, addr

#step 1 of the calibration: marker 1 is used to find T_RS_ML - transformation between realsense and Magicleap space.
def calibStep1(marker_type,  n_sq_y, n_sq_x, squareLen, markerLen, aruco_dict, c):
    #based on which marker type is chosen, the corresponding class instance is called.
   if marker_type == 1:
        boardinstance = charucocalibclass()
        ReturnFlag = 1
        T_RS_ML = boardinstance.startcamerastreaming(c, ReturnFlag,  n_sq_y, n_sq_x, squareLen, markerLen, aruco_dict)
        calibStep1Flag = True
    
   else:
        arucoinstance = arucocalibclass()
        ReturnFlag = 1
        T_RS_ML = arucoinstance.startcamerastreaming(c, ReturnFlag, markerLen, aruco_dict)
        calibStep1Flag = True
    return T_RS_ML, calibStep1Flag

#step 2 of calibration: computation of pose of marker 2 to realsense T_M2_RS
def calibStep2(T_RS_ML, c):
    # pose of marker 2 in Magicleap space: T_M2_ML sent by MagicLeap and received here
    dataRecv = c.recv(64)
    MLArr = np.frombuffer(dataRecv, dtype=np.float32)
    print("calib 2 MLArr", MLArr)
    T_M2_RS = None
    try:
        print("came to calib 2")
        #if array received is not Null
        if MLArr[3] != 0:
            print("received array")
            #reshape array as 4 x 4 matrix
            T_M2_ML = MLArr.reshape(4, 4)
            #compute T_M2_RS
            T_M2_RS = np.matmul(inv(T_RS_ML), T_M2_ML)
            calibStep2Flag = True
            #following lines are to convert the rotation to euler angles in order to save the rotation and translation in csv files.
            R_M2_ML_reshape = np.array(T_M2_ML[:3, :3]).reshape([3, 3])
            R_M2_ML = R.from_matrix(R_M2_ML_reshape)
            R_M2_ML_euler = R_M2_ML.as_euler('xyz', degrees=True)
            with open(file_name_TM2ML, 'a+', newline='') as write_obj:
                csv_writer = writer(write_obj)
                csv_writer.writerow([R_M2_ML_euler, T_M2_ML[:, 3].transpose()])
    except:
        print("Array not received from Magic Leap")
    return T_M2_RS, calibStep2Flag

# verification step of one-time calibration: T_M2_RS computed in step 2 is used along with T_M2_ML to compute T_RS_ML
def verifyT_RS_ML(marker_type, socket_connect,  n_sq_y, n_sq_x, squareLen, markerLen, aruco_dict, T_M2_RS, T_RS_ML1, c):
    print("came to verify calib part")
    #initialize T_M2_ML as zeros
    T_M2_ML = np.zeros([4, 4], np.float32)
    n_avg = 0
    N_samples = 3
    while (True):
        # get T_M2_ML from MagicLeap
        dataRecv = c.recv(64)
        MLArr = np.frombuffer(dataRecv, dtype=np.float32)
        print("verify MLArr", MLArr)
        if MLArr[3] != 0:
            # print(MLArr[3] != 0)
            T_M2_ML += MLArr.reshape(4, 4)
            with open(file_name_TM2ML, 'a+', newline='') as write_obj:
                csv_writer = writer(write_obj)
                # T_M2_ML
                csv_writer.writerow([T_M2_ML])
            print("T_M2_ML", T_M2_ML)
            n_avg += 1
            if n_avg == N_samples:
                #get T_M2_ML as average of three samples.
                T_M2_ML = T_M2_ML / N_samples
                print("T_M2_ML average", T_M2_ML)
                # save T_M2_ML is csv file
                with open(file_name_TM2ML, 'a+', newline='') as write_obj:
                    csv_writer = writer(write_obj)
                    # T_M2_ML
                    csv_writer.writerow([T_M2_ML])
                #compute T_RS_ML using T_M2_RS and T_M2_ML
                T_RS_ML2 = np.matmul(T_M2_ML, inv(T_M2_RS))
                #compare T_RS_ML computed in step 1 and in the verification step
                print("T_RS_ML in step 1 and verification step", T_RS_ML1, T_RS_ML2)
                # start tracking an object in Realsense space and use the transformation T_RS_ML2 to track in MagicLeap space
                startTracking(marker_type, T_RS_ML2,  n_sq_y, n_sq_x, squareLen, markerLen, aruco_dict, socket_connect, c)
                #break

def main():
    # set which marker type: 1 - Charucoboard, else - single aruco marker
    marker_type = 1
    # create a socket object if socket_connect = True
    socket_connect = True
    kalman_flag = False
    calibStep1Flag = False
    if marker_type == 1: #charuco board
        squareLen = 0.03710 # make changes to square length and marker length, number of squares based on the marker you are using for calibration
        markerLen = 0.02250
        n_sq_y = 7
        n_sq_x = 5
        aruco_dict = aruco.Dictionary_get(aruco.DICT_6X6_250)
    else: # single aruco
        squareLen = None
        markerLen = 0.061
        n_sq_y = None
        n_sq_x = None
        aruco_dict = aruco.Dictionary_get(aruco.DICT_4X4_250)

    # if socket_connect = True, call the respective calibration instance and claibrate with MagicLeap
    if socket_connect == True:
        c, addr = createSocket(socket_connect)
        #step 1 of calibration: marker 1 is used to find T_RS_ML - transformation between realsense and Magicleap space.
        T_RS_ML, calibStep1Flag = calibStep1(marker_type,  n_sq_y, n_sq_x, squareLen, markerLen, aruco_dict, c)
        print(T_RS_ML, calibStep1Flag)
        #following 3 lines are to convert to euler angles to save it in csv file
        R_RS_ML_reshape = np.array(T_RS_ML[:3,:3]).reshape([3, 3])
        R_RS_ML = R.from_matrix(R_RS_ML_reshape)
        R_RS_ML_euler = R_RS_ML.as_euler('xyz', degrees=True)

    else:
        #in case you want to test only the python code with realsense, make socket_connect=0
        T_RS_ML = np.array((1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1))
        T_RS_ML = T_RS_ML.reshape(4, 4)
        calibStep1Flag = True
        print("T_RS_ML", T_RS_ML, calibStep1Flag)
    # if calib step 1 is done, proceed to second step of calib, to compute T_M2_RS
    calibStep2Flag = False
    #Only if step 1 of calibration is done, proceed to step 2
    if socket_connect == True and calibStep1Flag == True:
        print("calib1 flag is true")
        # once step 1 of calibration is done, proceed to step 2
        while calibStep2Flag != True:
            print("proceeding to step 2")
            T_M2_RS, calibStep2Flag = calibStep2(T_RS_ML, c)
            print("T_M2_RS", T_M2_RS, calibStep2Flag)
            #following 7 lines are for saving T_M2_RS in csv file
            T_M2_RS_tosave = np.asarray(T_M2_RS)
            R_M2_reshape = np.array(T_M2_RS[:3,:3]).reshape([3, 3])
            R_M2 = R.from_matrix(R_M2_reshape)
            R_M2_euler = R_M2.as_euler('xyz', degrees=True)
            with open(file_name_TM2RS_Matrix, 'a+', newline='') as write_obj:
                csv_writer = writer(write_obj)
                csv_writer.writerow([T_M2_RS])
    #now, compute T_RS_ML using T_M2_RS and verify the calibration
    if calibStep2Flag == True:
        # save T_RS_ML and T_M2_RS to csv
        with open(file_name_TRSML, 'a+', newline='') as write_obj:
            csv_writer = writer(write_obj)
            csv_writer.writerow([R_RS_ML_euler, T_RS_ML[:, 3].transpose()])
        with open(file_name_TM2RS, 'a+', newline='') as write_obj:
            csv_writer = writer(write_obj)
            csv_writer.writerow([R_M2_euler, T_M2_RS[:, 3].transpose()])
        # verify calibration step
        marker_type = 3
        squareLen = None
        markerLen = 0.045
        n_sq_y = None
        n_sq_x = None
        aruco_dict = aruco.Dictionary_get(aruco.DICT_6X6_250)
        verifyT_RS_ML(marker_type, socket_connect,  n_sq_y, n_sq_x, squareLen, markerLen, aruco_dict, T_M2_RS, T_RS_ML, c)

    # When everything done, release the capture
    cv2.destroyAllWindows()
    s.close()

if __name__ == '__main__':
    main()
