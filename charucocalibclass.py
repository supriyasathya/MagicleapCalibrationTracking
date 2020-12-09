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
####------------------ ARUCO TRACKER ---------------------------
file_name_TM1ML = '/Users/supriya/Documents/Codes/Charuco_camera_calib/CharucoCalibration/TM1ML.csv'
file_name_TM1RS = '/Users/supriya/Documents/Codes/Charuco_camera_calib/CharucoCalibration/TM1RS.csv'

class charucocalibclass():
    
  #function to compute the transformation matrix that transforms a point in Realsense space to MagicLeap space
  def computecalibmatrix(self,MLArr,rvec,tvec, n_sq_y, n_sq_x, squareLen):
    #combine rvec and tvec too get the combined matrix of pose of an object in realsense space in homogeneous coordinates
    R_rvec = R.from_rotvec(rvec)
    R_rotmat = R_rvec.as_matrix()
    RSTr = np.hstack([R_rotmat[0],tvec.transpose()])
    RSTr = np.vstack([RSTr,[0,0,0,1]])
    print("RSTr", RSTr)
    
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
  
  # function to convert FSA pose to Aruco space (this function is called if FSANet is used for head pose estimation)
  def ConvertToArucoSys(self, RotMat, Posarr):
    # matrix to be multiplied in order to carry out axis flip.
    RotConvAruco = np.array([0,1,0,1,0,0,0,0,1])
    RotConvAruco = RotConvAruco.reshape(3, 3)
    RotMatTemp = np.matmul(RotMat, RotConvAruco)
    RotMat = np.matmul(inv(RotConvAruco), RotMatTemp)
    # now combine the rotation matrix with the Pos array
    Posarr = np.array(Posarr, np.float32)
    Posarr.reshape(1,3)
    print(RotMat.shape, Posarr.shape)
    RSFSATr = np.hstack([RotMat[0], Posarr.reshape(3,1)])
    RSFSATr = np.vstack([RSFSATr, [0, 0, 0, 1]])
    return RSFSATr, RotMat

  # function that starts Realsense camera streaming as well as detects charuco board pose and computes calibration matrix between realsense and Magicleap spaces
  def startcamerastreaming(self,c,ReturnFlag, n_sq_y, n_sq_x, squareLen, markerLen, aruco_dict):
     count = 0
     # start realsense camera streaming
     pipeline = rs.pipeline()
     config = rs.config()
     config.enable_stream(rs.stream.color,640,480,rs.format.bgr8,30)
     profile = pipeline.start(config)
     # get Realsense stream intrinsics
     intr = profile.get_stream(rs.stream.color).as_video_stream_profile().get_intrinsics()
     mtx = np.array([[intr.fx,0,intr.ppx],[0,intr.fy,intr.ppy],[0,0,1]])
     dist = np.array(intr.coeffs)
     #create charuco board with specified number of squares given marker length and square length and dictionary type.
     charuco_board = aruco.CharucoBoard_create( n_sq_x, n_sq_y, squareLen, markerLen, aruco_dict)
     rvec = np.zeros((1,3))  # , np.float32)
     tvec = np.zeros((1,3))  # , np.float32)
     calibDone = False
     MLRSSum = np.zeros((4,4))
     
     while (True):
             # start streaming from Realsense
                     frames = pipeline.wait_for_frames()
             
                     if calibDone != True:
                          print("came here")
                          # receive the marker 1 pose in Magicleap space.
                          dataRecv = c.recv(64)

                          MLArr = np.frombuffer(dataRecv,dtype=np.float32)
                          print(MLArr)
                          # if the marker 1 pose is not None, proceed to get marker 1 pose in Realsense space
                          if MLArr[3]!=0:
                              color_frame = frames.get_color_frame()
                              input_img = np.asanyarray(color_frame.get_data())
                            #  cv2.imshow('input',input_img)
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
                              if (ids is not None):
                                 ret, ch_corners, ch_ids = aruco.interpolateCornersCharuco(corners, ids, gray, charuco_board)
                                 # if there are enough corners to get a reasonable result
                                 if (ret > 3):
                                     aruco.drawDetectedCornersCharuco(input_img, ch_corners, ch_ids, (0, 0, 255))

                                     retval, rvec, tvec = cv2.aruco.estimatePoseCharucoBoard(ch_corners, ch_ids, charuco_board, mtx, dist, None, None)
                                     # if the board pose could be estimated, continue with remaining steps
                                     if( retval ) :
                                         print("board pose", rvec, tvec)
                                         rvec = np.reshape(rvec, (1,3))
                                         tvec= np.reshape(tvec, (1,3))

                                         print("board pose", rvec, tvec)
                                         # draw charuco board axis
                                         aruco.drawAxis(input_img, mtx, dist, rvec, tvec, 0.1)

                                         print(MLArr, rvec, tvec)
                                         # compute the calibration matrix between realsense space and Magicleap space for the 'i'th sample of marker 1 pose in realsense and Magicleap space.
                                         MLRSi = self.computecalibmatrix(MLArr,rvec, tvec, n_sq_y, n_sq_x, squareLen)
                                         print("MLRSi", MLRSi)
                                          # convert MLRSi to euler
                                          #R_M1_RS_reshape = np.array(MLRSi[:3,:3]).reshape([3,3])
                                         R_M1_RS = R.from_rotvec(rvec)
                                         R_M1_RS_euler = R_M1_RS.as_euler('xyz', degrees=True)
                                          # save the MLRSi values in csv
                                         with open(file_name_TM1RS, 'a+', newline='') as write_obj:
                                             csv_writer = writer(write_obj)
                                             csv_writer.writerow([R_M1_RS_euler, tvec.reshape([1,3])])
                                         MLRSSum = MLRSSum + MLRSi
                                         count = count+1

                                         cv2.imshow('frame', input_img)
                              #number of samples for homography = 15
                              if count >= 15:
                                 print("count",count)
                                 MLRSFinal = MLRSSum/count
                                 #once 15 homography matrices are computed, their average is computed and retuened as the final transformation matrix from Realsense to MagicLeap space.
                                 print("MLRSFinal", MLRSFinal)
                                 print("calib done")
                                 calibDone = True
                                 if ReturnFlag == 1:
                                    return MLRSFinal
                     
                     if cv2.waitKey(1) & 0xFF == ord('q'):
                        break

         
 
                  # When everything done, release the capture
                  #cap.release()
                     cv2.destroyAllWindows()






         
     


          





























#cap = cv2.VideoCapture(0)

## realsense stuff



