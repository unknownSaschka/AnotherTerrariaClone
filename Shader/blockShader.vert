#version 330

layout (location = 0) in vec2 position;
layout (location = 1) in vec2 texCoordinate;
layout (location = 2) in float bDarkness;

out vec2 texCoord;
out float blockDarkness;


uniform mat4 transform;
uniform vec4 translation;

void main()
{
    blockDarkness = bDarkness;
    texCoord = texCoordinate;
    gl_Position = transform * (vec4(position, 0.0, 1.0) + translation);
}