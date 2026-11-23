"""Contract tests for v.YY.MM.vv and NuGet normalization."""
import unittest
from check_version import parts, nuget_normalized


class VersioningTests(unittest.TestCase):
    def test_current_version(self):
        self.assertEqual(parts("0.26.11.1"), (0, 26, 11, 1))

    def test_major_and_monthly_bump(self):
        self.assertEqual(parts("1.26.12.3"), (1, 26, 12, 3))

    def test_january_nuget_normalization(self):
        self.assertEqual(nuget_normalized("1.27.01.1"), "1.27.1.1")

    def test_september_nuget_normalization(self):
        self.assertEqual(nuget_normalized("0.27.09.12"), "0.27.9.12")

    def test_invalid_values(self):
        bad = ("v1.26.11.1", "1.2026.11.1", "1.26.00.1",
               "1.26.13.1", "1.26.1.1", "1.26.11.0",
               "1.26.11.01", "01.26.11.1", "1.26.11.1-alpha")
        for value in bad:
            with self.subTest(value=value):
                with self.assertRaises(ValueError):
                    parts(value)


if __name__ == "__main__":
    unittest.main()
