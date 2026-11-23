"""Regression tests preventing literal ${ github.* } in workflows."""
import unittest

from check_workflows import validate_expressions


class GithubExpressionsTests(unittest.TestCase):
    def test_wrong_single_brace_expression_is_rejected(self):
        samples = [
            "ref: ${ github.event.workflow_run.head_sha }",
            "env:\n  GH_TOKEN: ${github.token}",
            "group: release-${ inputs.release_tag || github.event.release.tag_name }",
        ]
        for sample in samples:
            with self.subTest(sample=sample):
                with self.assertRaises(ValueError):
                    validate_expressions(sample)

    def test_github_double_braces_are_valid(self):
        validate_expressions("ref: ${{ github.event.workflow_run.head_sha }}")

    def test_shell_expansions_remain_valid(self):
        validate_expressions('run: |\n  version="${VERSION_TAG#v}"\n  echo "${flags[@]}"')


if __name__ == "__main__":
    unittest.main()
