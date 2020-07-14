#version 330 core

in vec2 texCoord;
in float blockDarkness;

uniform vec4 blockColor;
uniform sampler2D texture0;

out vec4 color;


void main()
{
    color = (texture(texture0, texCoord) * blockColor) * vec4(blockDarkness, blockDarkness, blockDarkness, 1.0);
}