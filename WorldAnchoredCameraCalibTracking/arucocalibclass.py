

import numpy as np
import cv2
import cv2.aruco as aruco
import os
import pickle
import pyrealsense2 as rs
import matplotlib.pyplot as plt 
import struct
from scipy.spatial.transform import Rotation as R
from numpy.linalg import inv
from csv import writer


file_name_TM1ML = '/home/supriya/supriya/FSA-Net/demo/TM1ML.csv'
file_name_TM1RS = '/home/supriya/supriya/FSA-Net/demo/TM1RS.csv'
class arucocalibclass():

  #function to compute the transformation matrix that transforms a point in Realsense space to MagicLeap space
  def computecalibmatrix(self,MLArr,rvec,tvec):
    print("rvec, tvec", rvec, tvec)
    #combine rvec and tvec too get the combined matrix of pose of an object in realsense space in homogeneous coordinates
    R_rvec = R.from_rotvec(rvec)
    R_rotmat = R_rvec.as_matrix()
    RSTr = np.hstack([R_rotmat[0],tvec.transpose()])
    RSTr = np.vstack([RSTr,[0,0,0,1]])
    # Since pose detected in OpenCV will be right handed coordinate system, it needs to be converted to left-handed coordinate system of Unity
    RSTr_LH = np.array([RSTr[0][0],RSTr[0][2],RSTr[0][1],RSTr[0][3],RSTr[2][0],RSTr[2][2],RSTr[2][1],RSTr[2][3],RSTr[1][0],RSTr[1][2],RSTr[1][1],RSTr[1][3],RSTr[3][0],RSTr[3][1],RSTr[3][2],RSTr[3][3]])# converting to left handed coordinate system
    RSTr_LH = RSTr_LH.reshape(4,4)
    # pose of marker 1 in MagicLeap space reshaped into a 4 x 4 matrix
    M1_ML = MLArr.reshape(4,4)
    # transform of a point from Realsense space to Magicleap space
    MLRS = np.matmul(M1_ML,inv(RSTr_LH))
    #following 7 lines are to save M1_ML in a csv file
    R_M1_ML_reshape = M1_ML[:3, :3].reshape([3, 3])
    print("R_M1_ML", R_M1_ML_reshape)
    R_M1_ML = R.from_matrix(R_M1_ML_reshape)
    R_M1_ML_euler = R_M1_ML.as_euler('xyz', degrees=True)
    with open(file_name_TM1ML, 'a+', newline='') as write_obj:
        csv_writer = writer(write_obj)
        csv_writer.writerow([R_M1_ML_euler, M1_ML[:, 3].transpose()])
    return MLRS
  
  # function that starts Realsense camera streaming as well as detects aruco marker and computes calibration matrix between realsense and Magicleap spaces
  def startcamerastreaming(self,c,ReturnFlag, markerLen, aruco_dict):
     print("came to aruco")
     count = 0
     # start realsense camera streaming
     pipeline = rs.pipeline()
     config = rs.config()
     config.enable_stream(rs.stream.color,640,480,rs.format.bgr8,30)
     profile = pipeline.start(config)
     # get Realsense stream intrinsics
     intr = profile.get_stream(rs.stream.color).as_video_stream_profile().get_intrinsics()
     intr.ppx
     mtx = np.array([[intr.fx,0,intr.ppx],[0,intr.fy,intr.ppy],[0,0,1]])
     dist = np.array(intr.coeffs)
     calibDone = False
     MLRSSum = np.zeros((4,4))
     while (True):
             # start streaming from Realsense
             frames = pipeline.wait_for_frames()
             color_frame = frames.get_color_frame()
             input_img = np.asanyarray(color_frame.get_data())
             
            # operations on the frame - conversion to grayscale
             gray = cv2.cvtColor(input_img, cv2.COLOR_BGR2GRAY)
            
            # detector parameters can be set here (List of detection parameters[3])
             parameters = aruco.DetectorParameters_create()
             parameters.adaptiveThreshConstant = 10

            # lists of ids and the corners belonging to each id
             corners, ids, rejectedImgPoints = aruco.detectMarkers(gray, aruco_dict, parameters=parameters)
            # font for displaying text (below)
             font = cv2.FONT_HERSHEY_SIMPLEX
            # check if the ids list is not empty
            # if no check is added the code will crash
                     
             if np.all(ids != None):
                 # estimate pose of each marker and return the values
                 rvec, tvec ,_ = aruco.estimatePoseSingleMarkers(corners, markerLen, mtx, dist)
                     
                 for i in range(0, ids.size):
                      # draw axis for the aruco markers
                     aruco.drawAxis(input_img, mtx, dist, rvec[i], tvec[i], 0.1)
                                 
                # draw a square around the markers
                 aruco.drawDetectedMarkers(input_img, corners)
                 cv2.imshow('frame',input_img)

                 if calibDone != True:
                     #receive mmarker 1 pose from Magicleap
                     dataRecv = c.recv(64)
                     MLArr = np.frombuffer(dataRecv,dtype=np.float32)
                     print(MLArr)
                     #if the received array is non-zero,
                     if MLArr[3]!=0:
                        count = count+1
                        #then ptoceed with computing the transformation matrix for every 'i'th sample of marker 1's pose
                        MLRSi = self.computecalibmatrix(MLArr,rvec[0], tvec[0])
                        print("MLRSi", MLRSi)
                        #sum the transformation matrices for every i'th sample and then take their average once desired number of samples are taken
                        MLRSSum = MLRSSum + MLRSi
                        print("MLRSSum", MLRSSum)
                        # following 7 lines are for saving pose of Marker 1 in Realsense space
                        R_M1_RS = R.from_rotvec(rvec[0])
                        R_M1_RS_euler = R_M1_RS.as_euler('xyz', degrees=True)
                        # save the MLRSi values in csv
                        with open(file_name_TM1RS, 'a+', newline='') as write_obj:
                             csv_writer = writer(write_obj)
                             csv_writer.writerow([R_M1_RS_euler, tvec[0].reshape([1, 3])])
                        # take the average of the transformation matrix once desired number of samples are taken.
                        if count >= 10:
                            print("count",count)
                            MLRSFinal = MLRSSum/count
                            print("MLRSFinal", MLRSFinal)
                            print("calib done")
                            calibDone = True
                            if ReturnFlag == 1:
                                #return the computed transformation matrix between Realsense and Magicleap back to the calling code
                                return MLRSFinal
                 
                                                                                         
                                                                                         
             if cv2.waitKey(1) & 0xFF == ord('q'):
                  break
                                                                                                 
              # When everything done, release the capture
             cv2.destroyAllWindows()
