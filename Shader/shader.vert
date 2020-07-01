﻿#version 330 core

in vec3 aPosition;
in vec2 aTexCoord;

uniform vec3 changedPosition;
uniform mat4 transform;

void main (void)
{
	gl_Position = vec4(aPosition, 1.0) * transform;
}