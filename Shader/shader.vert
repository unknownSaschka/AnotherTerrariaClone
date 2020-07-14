#version 330 core

layout (location = 0) in vec4 vertex; // <vec2 pos, vec2 tex>


out vec2 texCoord;
  
uniform mat4 transform;
uniform vec4 translation;


void main()
{
    texCoord = vertex.zw;    
    gl_Position = transform * (vec4(vertex.xy, 0.0, 1.0) + translation);
}