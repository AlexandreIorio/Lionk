import os
# get environment variables
LIB_PATH = os.getenv("LIB_PATH")


#  get project names from projects.txt
with open("projects.txt", "r") as file:
    projects = file.read().strip().split()

# backup each project file
for project in projects:
    src_file = os.path.join(LIB_PATH, project, f"{project}.csproj")
    backup_file = os.path.join(LIB_PATH, project, f"{project}.csproj.bkp")

    with open(src_file, "rb") as src:
        content = src.read()

    with open(backup_file, "wb") as bkp:
        bkp.write(content)
