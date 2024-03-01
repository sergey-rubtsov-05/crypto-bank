workspace {

    model {
        customer = person "Customer" "A user of my crypto-bank system." {
            tags "External"
        }
        admin = person "Admin" "A user which administrate my crypto-bank system"

        cryptoBank = softwareSystem "Crypto Bank" "My crypto-bank system." {

            customerFrontend = container "Customer Frontend" "Any frontend app which customer can use to interact with the system." {
                customer -> this
            }

            backOfficeFrontend = container "Back Office Frontend" "Any frontend app which admin can use to interact with the system." {
                admin -> this
            }

            userService = container "User Service" "User service for my crypto-bank system." {
                userRegistration = component "User Registration" "User registration component." {
                    customerFrontend -> this
                }
                userAuth = component "User Auth" "User authentication component." {
                    customerFrontend -> this
                }
                userProfile = component "User Profile" "User account component." {
                    customerFrontend -> this
                }
                updatePassword = component "Update Password" "Update password component." {
                    customerFrontend -> this
                }
                updateRoles = component "Update Roles" "Update roles component." {
                    backOfficeFrontend -> this
                }
            }

            accountService = container "Account Service" "Account service for my crypto-bank system." {
                createAccount = component "Create Account" "Create account component." {
                    customerFrontend -> this
                }
                getCustomerAccounts = component "Get Customer Accounts" "Get list of customer accounts." {
                    customerFrontend -> this
                }
                transferFundsBetweenAccounts = component "Transfer Funds" "Transfer funds between accounts." {
                    customerFrontend -> this
                }
                createVirtualCardAccount = component "Create Virtual Card Account" "Create virtual card with account component." {
                    customerFrontend -> this
                }
                getCustomerVirtualCards = component "Get Customer Virtual Cards" "Get list of customer virtual cards." {
                    customerFrontend -> this
                }
                transferFundsToCardFromAccount = component "Transfer Funds To Card From Account" "Transfer funds from account to virtual card." {
                    customerFrontend -> this
                }
                transferFundsToAccountFromCard = component "Transfer Funds To Account From Card" "Transfer funds from virtual card to account." {
                    customerFrontend -> this
                }
                transferFundsBetweenCards = component "Transfer Funds Between Cards" "Transfer funds between virtual cards." {
                    customerFrontend -> this
                }
                getTransfersHistory = component "Get Transfers History" "Get transfers history for account." {
                    customerFrontend -> this
                }
            }

            qrPaymentService = container "QR Payment Service" "QR payment service for my crypto-bank system." {
                generateQr = component "Generate QR" "Generate QR code for payment." {
                    customerFrontend -> this
                }
                scanQr = component "Scan QR" "Scan QR code for payment." {
                    customerFrontend -> this
                }
                getQrPaymentsHistory = component "Get QR Payments History" "Get payments history for account." {
                    customerFrontend -> this
                }
            }

            btcBlockchainService = container "BTC Blockchain Service" "BTC blockchain service for my crypto-bank system." {
                createDepositAddressBtc = component "Create Deposit Address" "Create deposit address for account." {
                    customerFrontend -> this
                }
                getDepositBtc = component "Get Deposit" "Get deposit for account." {
                    customerFrontend -> this
                }
                approveDepositBtcJob = component "Approve Deposit Job" "Approve deposit for account."
                createWithdrawalBtc = component "Create Withdrawal" "Create withdrawal for account." {
                    customerFrontend -> this
                }
                approveWithdrawalBtcJob = component "Approve Withdrawal Job" "Approve withdrawal for account."
            }

            ethBlockchainService = container "ETH Blockchain Service" "ETH blockchain service for my crypto-bank system." {
                createDepositAddressEth = component "Create Deposit Address" "Create deposit address for account." {
                    customerFrontend -> this
                }
                getDepositEth = component "Get Deposit" "Get deposit for account." {
                    customerFrontend -> this
                }
                approveDepositEthJob = component "Approve Deposit Job" "Approve deposit for account."
                createWithdrawalEth = component "Create Withdrawal" "Create withdrawal for account." {
                    customerFrontend -> this
                }
                approveWithdrawalEthJob = component "Approve Withdrawal Job" "Approve withdrawal for account."
            }

            utilityProviderService = container "Utility Provider Service" "Utility provider service for my crypto-bank system." {
                loadUnpaidBillsJob = component "Load Unpaid Bills Job" "Periodically load unpaid bills for account." {
                }
                getUppaidBills = component "Get Unpaid Bills" "Get unpaid bills for account." {
                    customerFrontend -> this
                }
                payBill = component "Pay Bill" "Pay bill for account." {
                    customerFrontend -> this
                }
                getPaymentsHistory = component "Get Payments History" "Get payments history for account." {
                    customerFrontend -> this
                }
            }

            reportService = container "Report Service" "Report service for my crypto-bank system." {
                generateOpennedAccountsForPeriodReport = component "Generate Openned Accounts For Period Report" "Generate openned accounts for period report." {
                    backOfficeFrontend -> this
                }
                generateCryptoCurrencyInOutReport = component "Generate Crypto Currency In Out Report" "Generate crypto currency in out report." {
                    backOfficeFrontend -> this
                }
            }

            notificationService = container "Notification Service" "Notification service for my crypto-bank system." {
                createNotification = component "Send Notification" "Send notification for customer." {
                    getDepositBtc -> this "Publish async notification" "" "Async"
                    approveDepositBtcJob -> this "Publish async notification" "" "Async"
                    approveWithdrawalBtcJob -> this "Publish async notification" "" "Async"
                    getDepositEth -> this "Publish async notification" "" "Async"
                    approveDepositEthJob -> this "Publish async notification" "" "Async"
                    approveWithdrawalEthJob -> this "Publish async notification" "" "Async"
                    loadUnpaidBillsJob -> this "Publish async notification" "" "Async"
                }

                sendEmailNotification = component "Send Email Notification" "Send email notification for customer." {
                    createNotification -> this
                }

                sendSignalRNotification = component "Send SignalR Notification" "Send signalR notification for customer." {
                    createNotification -> this
                }
            }
        }

        softwareSystem "BTC blockchain" {
            tags "External"
            btcBlockchainService -> this "Uses"
        }

        softwareSystem "ETH blockchain" {
            tags "External"
            ethBlockchainService -> this "Uses"
        }

        utilityProvider = softwareSystem "UtilityProvider" "Providers features for get and pay utility bills" {
            tags "External"
            utilityProviderService -> this "Uses"
        }
    }

    views {
        systemContext cryptoBank "SystemContext" {
            include *
            autoLayout
        }

        container cryptoBank {
            include *
            autoLayout
        }

        component userService {
            include *
            autoLayout
        }

        component accountService {
            include *
            autoLayout
        }

        component qrPaymentService {
            include *
            autoLayout
        }

        component btcBlockchainService {
            include *
            autoLayout
        }

        component ethBlockchainService {
            include *
            autoLayout
        }

        component utilityProviderService {
            include *
            autoLayout
        }

        component reportService {
            include *
            autoLayout
        }

        component notificationService {
            include *
            autoLayout
        }

        theme default

        styles {
            element "External" {
                background #dddddd
                color #000000
            }

            relationship "Async" {
                style dotted
            }
        }
    }

}