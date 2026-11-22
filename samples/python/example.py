"""
DroidBox Automation Sample Script
Demonstrates headless / scripted testing of Android APKs using DroidBox.
"""

from droidbox import DroidBox

def main():
    print("=== DroidBox Python Automation Demo ===")
    client = DroidBox()

    # 1. Check environment health
    print("\n1. Running DroidBox Doctor...")
    health = client.doctor()
    print(health["stdout"])

    # 2. List available virtual device profiles
    print("\n2. Querying Virtual Device Profiles...")
    profiles = client.devices()
    print(profiles["stdout"])

    # 3. Query installed applications
    print("\n3. Listing Installed Applications...")
    apps = client.list_apps()
    print(apps["stdout"])

    print("\n=== Demo Complete ===")

if __name__ == "__main__":
    main()
