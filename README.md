# benjis controller relay software 

## Before i talk about it heres my TODOS: 
  relaying from osc to virtual hid device
  configuration with file and not from code 
  make a more beaut ui without text as the debug but actual cotroller visualisation

### Now to the actual information 
#### how to build from source: (this requires .NET 10 SDK)
clone the repo and clone the additional repos to the folder 
`git clone https://github.com/bbuny007/osc_controller_relay.buny`
and the others too
`git clone https://github.com/VolcanicArts/FastOSC`
`git clone https://github.com/raylib-extras/rlImGui-cs`
any other lib is from nuget so already included
then cd into the code dir and run your build command
Linux:
Windows:
Macos:

# FAQ
## Why make one yourself if there are already so many?
(for context ppl on discord dm me with similar projects)
1. i thought it would be a good first project
2. i couldnt find a programm that also did it backwards like took osc data and put it into a virtual hid device like an controller (still a todo though)
## Why such code in one commit?
i fogor to make a repo sorry

## AI usage disclosure

i may or may not have used a lil claude (free plan ofc) to
code me the controller handler (because im lazy and dont want to copy paste alot for the diffrent buttons)
and to help me structure my code better (nothing rewritten just basic codeing knowledge)

So basically instead of googling i used the awnsers of claude sorry if this repells some of yall but it is what it is and im too socially aquawrd to ask simple questions to more knowledged individuals

# Contribution policies

anything is appreciated.

## Licensing

This project is licensed under the MIT License — see LICENSE for details.

# Configuriration
This programm currently can be configured through the code only there is a struct in Config.cs with values to set



### Third Party Libraries

 Library         License  
 ImGui.NET       MIT      
 Raylib-cs       Zlib     
 SDL2            Zlib     
 SDL2-CS.NetCore MIT      
 rlImGui-cs      MIT      
 FastOSC         LGPL-3.0 

See lisences folder for full license texts.
FastOSC is dynamically linked and its source is available at
https://github.com/VolcanicArts/FastOSC