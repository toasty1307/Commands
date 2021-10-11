from io import FileIO
import os
import os.path
import pathlib
import subprocess

output_dir = "Min"
final_output = ""
csproj_code = """
            <Project Sdk="Microsoft.NET.Sdk">
                <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>net6.0</TargetFramework>
                </PropertyGroup>
    
                <ItemGroup>
                <None Update="Config.json">
                    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                </None>
                </ItemGroup>
                <ItemGroup>
                    <PackageReference Include="DSharpPlus" Version="4.2.0-nightly-01008" />
                    <PackageReference Include="Microsoft.CodeAnalysis.CSharp.Scripting" Version="4.0.0-3.final" />
                    <PackageReference Include="DSharpPlus.Lavalink" Version="4.2.0-nightly-01008" />
                    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.0-rc.1.21452.10" />
                    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="6.0.0-rc.1" />
                </ItemGroup>
            </Project>
            """
config_code = """
              {
                  "Token" : "tokenhere",
                  "Prefix" : "pls "
              }
              """

def do_more_thing():
    print("Writing stuff")
    global final_output
    global output_dir
    global csproj_code
    global config_code
    min_project_path = os.path.join(pathlib.Path(__file__).parent.resolve(), output_dir)
    min_file_path = os.path.join(min_project_path, f"{output_dir}.cs")
    min_csproj_path = os.path.join(min_project_path, f"{output_dir}.csproj")
    min_config_path = os.path.join(min_project_path, "Config.json")
    os.mkdir(min_project_path)
    with open(min_file_path, 'w') as f:
        f.write(final_output)
    with open(min_csproj_path, 'w') as f:
        f.write(csproj_code)
    with open(min_config_path, 'w') as f:
        f.write(config_code)

    print("done writing")

def do_thing():
    global output_dir
    global final_output
    this_dir = pathlib.Path(__file__).parent.resolve()
    for root, _, files in os.walk(this_dir):
        for file in filter(lambda x : x.endswith(".cs"), files):
            if not ("obj" in root or "bin" in root):
                print("minifying " + file)
                output = subprocess.check_output(['csmin', os.path.join(root, file)])
                output = output.decode("utf-8").replace("\r", "").replace("\n", "")
                final_output += output

    do_more_thing()

if __name__ == "__main__":
    do_thing()