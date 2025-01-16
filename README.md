# PID-Demo
Proportional Integral Derivative Demo in C#

### What is PID?
PID stands for Proportional-Integral-Derivative. It is a control loop feedback mechanism commonly used in industrial control systems and automation. The purpose of PID is to continuously calculate an error value and apply a correction based on three terms:

- **Proportional (P or Kp):** This term depends on the current error. The larger the error, the larger the corrective action.
- **Integral (I or Ki):** This term accounts for the accumulation of past errors, helping eliminate any residual steady-state error.
- **Derivative (D or Kd):** This term considers the rate of change of the error, anticipating future errors and smoothing out the control signal.


***Simplified:***
- *Higher Kp = Faster response, but might overshoot.*
- *Lower Kp = Slower response, but no overshooting.*
- *Higher Ki = Fixes small errors, but may overcorrect.*
- *Lower Ki = Leaves small errors, but smoother behavior.*
- *Higher Kd: Smooths out movements by dampening rapid changes, making the system slower but more stable.*
- *Lower Kd: Leads to less damping, making the system respond faster but bouncier or more oscillatory.*

#
This project demonstrates the use of a simple PID controller to move a circle toward a target position (Red Circle). The system adjusts the circle's position using proportional, integral, and derivative control to achieve smooth and accurate movement. The application is built using **C#** and Windows Forms.

## Features
- Smooth circle movement using PID control.
- Adjustable PID values via text inputs for real-time tuning.
- Visual feedback with a draggable circle and a target position.


#
*Note: Please slowly increment the values in decimal point as higher values may lead to overshoot and glitches.*
#
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
