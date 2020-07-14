#version 330 core

in vec2 texCoord;
out vec4 color;

uniform sampler2D texture0;
uniform vec4 blockColor;

void main()
{
    color = texture(texture0, texCoord) * blockColor;
}
