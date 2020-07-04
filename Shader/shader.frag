#version 330 core

uniform vec4 blockColor;

in vec2 texCoord;
out vec4 color;

uniform sampler2D texture0;

void main()
{
    color = texture(texture0, texCoord);
}