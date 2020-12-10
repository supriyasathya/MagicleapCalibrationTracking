import numpy as np 
from math import sqrt 
from sklearn.metrics import mean_squared_error
from scipy.spatial.transform import Rotation as R


#script to compute error or change in pose between two transformation matrices. In the accuracy measurement experiments, A1 is the pose of a cube before and A2 is the pose after any manual adjustment of the virtual rendering to align with the real-world counterpart.
#Replace A1 and A2 with transoformation matrices we want to find the error between.

A1 = np.array([[ 0.99425059, -0.09569998, -0.02008072, -0.33576568,], [-0.01647349, 0.03688412, -0.99904052, -0.0432022, ], [ 0.0963928, 0.99377417, 0.03480588, -0.25319909,], [ 0., 0., 0., 1., ],])
A2 = np.array([[ 0.99421707, -0.0958724, -0.02089681, -0.33503756,], [-0.01716557, 0.03812297, -0.99898238, -0.04281027,], [ 0.09661551, 0.99371053, 0.03596903, -0.25362241,], [ 0., 0., 0., 1., ],])
A1 = A1.reshape(4,4)
A2 = A2.reshape(4,4)


A1rot = A1[:3,:3]
A2rot = A2[:3,:3]

A1tra = A1[:3,3]
A2tra = A2[:3,3]


A1R = R.from_matrix(A1rot)
A2R = R.from_matrix(A2rot)

A1euler = A1R.as_euler('xyz', degrees=True)
A2euler = A2R.as_euler('xyz',degrees= True)

rmsrot = sqrt(mean_squared_error(A1euler,A2euler))
rmstra = sqrt(mean_squared_error(A1tra,A2tra))
print(A1euler, A2euler)
print("rmsrot", rmsrot)
print("rmstra", rmstra)


