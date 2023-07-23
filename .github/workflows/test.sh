#!/bin/bash

TEST_VALUE="Hello Variable!"
echo "MyTestVar="$TEST_VALUE" >> $GITHUB_ENV

echo "Retreived Value ${{ env.MyTestVar }}"
