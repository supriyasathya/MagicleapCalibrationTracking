# charuco_pose.py
#
# Peter F. Klemperer
# October 29, 2017
import pdb
import time
import numpy as np
import cv2
import glob

import cv2.aruco as aruco
import numpy as np
import cv2
print(cv2.__version__)
import cv2.aruco as aruco
import os
import pickle
import pyrealsense2 as rs
import matplotlib.pyplot as plt
import struct
from scipy.spatial.transform import Rotation as R
from numpy.linalg import inv

#script to detect charuco board pose using Realsense.

def read_node_real( reader, name ):
 node = reader.getNode( name )
 return node.real()

def read_node_string( reader, name ):
 node = reader.getNode( name )
 return node.string()

def read_node_matrix( reader, name ):
 node = reader.getNode( name )
 return node.mat()

# read defaultConfig.xml to extract the charuco board parameters
config_reader = cv2.FileStorage()
config_reader.open("defaultConfig.xml",cv2.FileStorage_READ)

aruco_parameters = aruco.DetectorParameters_create()
aruco_dict_num = int(read_node_real( config_reader, "charuco_dict" ) )
aruco_dict = aruco_dict = aruco.Dictionary_get(aruco.DICT_6X6_1000)#aruco.Dictionary_get(aruco_dict_num)
charuco_square_length = int(read_node_real( config_reader, "charuco_square_length" ))
charuco_marker_size = int(read_node_real( config_reader, "charuco_marker_size" ))
config_reader.release()

charuco_board = aruco.CharucoBoard_create(5,7, 0.034750, 0.0210, aruco_dict)

count = 0
#start streaming from realsense
pipeline = rs.pipeline()
config = rs.config()
config.enable_stream(rs.stream.color, 640, 480, rs.format.bgr8, 30)
profile = pipeline.start(config)
#get intrinsics of the stream
intr = profile.get_stream(rs.stream.color).as_video_stream_profile().get_intrinsics()
#get the camera matrix and distorsion coefficients
mtx = np.array([[intr.fx, 0, intr.ppx], [0, intr.fy, intr.ppy], [0, 0, 1]])
dist = np.array(intr.coeffs)
#initialize rotation and translation vectors rvec and tvec
rvec = np.zeros((1,3))#, np.float32)
tvec = np.zeros((1,3))#, np.float32)
while(True):
  time.sleep( 0.1 )
  # Read frame from realsense
  frames = pipeline.wait_for_frames()
  color_frame = frames.get_color_frame()
  input_img = np.asanyarray(color_frame.get_data())
  #  cv2.imshow('input',input_img)
  # convert frame to grayscale
  gray = cv2.cvtColor(input_img, cv2.COLOR_BGR2GRAY)
  # detect Aruco markers
  corners, ids, rejectedImgPoints = aruco.detectMarkers(gray, aruco_dict,
  parameters=aruco_parameters)

 # if enough markers were detected, then process the board
  if( ids is not None ):
    ret, ch_corners, ch_ids = aruco.interpolateCornersCharuco(corners, ids, gray, charuco_board)
   
  # if there are enough corners to get a reasonable result, proceed to estimate board pose
    if( ret > 3 ):
      aruco.drawDetectedCornersCharuco(input_img,ch_corners,ch_ids,(0,0,255))

      retval, rvec, tvec = cv2.aruco.estimatePoseCharucoBoard(ch_corners, ch_ids, charuco_board, mtx, dist, None, None)#, useExtrinsicGuess=False)

      print("pose", retval, rvec, tvec)
      #In the next few lines of code, the transformation matrix of Aruco board pose is obtained by combining tvec and rvec.
      rvec = np.reshape(rvec, (1,3))
      tvec= np.reshape(tvec, (1,3))
      R_rvec = R.from_rotvec(rvec)
      R_rotmat = R_rvec.as_matrix()
      R_euler = R_rvec.as_euler('xyz', degrees= True)
      print("R_euler", R_euler)
      RSTr = np.hstack([R_rotmat[0],tvec.transpose()])
      RSTr = np.vstack([RSTr,[0,0,0,1]])
      print("RSTr", RSTr)
      #draw axis showing board pose
      aruco.drawAxis(input_img,mtx,dist,rvec,tvec,0.1)

  # imshow and waitKey are required for the window to open on a mac.
  cv2.imshow('frame', input_img)


  if( cv2.waitKey(1) & 0xFF == ord('q') ):
    break

cv2.destroyAllWindows()
