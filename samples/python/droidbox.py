"""
DroidBox Python Client SDK
Lightweight Android Testing Automation for Windows
"""

import json
import subprocess
from typing import Optional, Dict, Any, List

class DroidBox:
    """Python wrapper for the DroidBox CLI and IPC runtime."""

    def __init__(self, cli_path: str = "droidbox.exe"):
        self.cli_path = cli_path

    def _run(self, *args: str) -> Dict[str, Any]:
        cmd = [self.cli_path] + list(args)
        proc = subprocess.run(cmd, capture_output=True, text=True)
        return {
            "returncode": proc.returncode,
            "stdout": proc.stdout.strip(),
            "stderr": proc.stderr.strip(),
            "success": proc.returncode == 0
        }

    def doctor(self) -> Dict[str, Any]:
        """Check the system health and Android SDK environment."""
        return self._run("doctor")

    def status(self) -> Dict[str, Any]:
        """Check current emulator and device status."""
        return self._run("status")

    def devices(self) -> Dict[str, Any]:
        """List available virtual device profiles."""
        return self._run("devices")

    def install(self, apk_path: str) -> Dict[str, Any]:
        """Install an APK into the virtual Android environment."""
        return self._run("install", apk_path)

    def launch(self, package_name: str) -> Dict[str, Any]:
        """Launch an application by its package name."""
        return self._run("launch", package_name)

    def uninstall(self, package_name: str) -> Dict[str, Any]:
        """Uninstall an application by package name."""
        return self._run("uninstall", package_name)

    def list_apps(self) -> Dict[str, Any]:
        """List all installed applications."""
        return self._run("apps")

    def screenshot(self, output_path: str) -> Dict[str, Any]:
        """Capture the screen of the active virtual device."""
        return self._run("screenshot", "--output", output_path)

    def stop(self) -> Dict[str, Any]:
        """Stop running emulator instances."""
        return self._run("stop")
