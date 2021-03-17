namespace Namespace {
    
    using unittest;
    
    using DVVSet = dvvset.DVVSet;
    
    using Clock = dvvset.Clock;
    
    using System.Collections.Generic;
    
    public static class Module {
        
        public class TestDVVSet
            : unittest.TestCase {
            
            public virtual object setUp() {
                this.dvvset = DVVSet();
            }
            
            public virtual object test_join() {
                var A = this.dvvset.@new("v1");
                var A1 = this.dvvset.create(A, "a");
                var B = this.dvvset.new_with_history(this.dvvset.join(A1), "v2");
                var B1 = this.dvvset.update(B, A1, "b");
                this.assertEqual(this.dvvset.join(A), new List<object>());
                this.assertEqual(this.dvvset.join(A1), new List<object> {
                    new List<object> {
                        "a",
                        1
                    }
                });
                this.assertEqual(this.dvvset.join(B1), new List<object> {
                    new List<object> {
                        "a",
                        1
                    },
                    new List<object> {
                        "b",
                        1
                    }
                });
            }
            
            public virtual object test_update() {
                var A0 = this.dvvset.create(this.dvvset.@new("v1"), "a");
                var A1 = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(A0), new List<object> {
                    "v2"
                }), A0, "a");
                var A2 = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(A1), new List<object> {
                    "v3"
                }), A1, "b");
                var A3 = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(A0), new List<object> {
                    "v4"
                }), A1, "b");
                var A4 = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(A0), new List<object> {
                    "v5"
                }), A1, "a");
                this.assertEqual(A0, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            1,
                            new List<object> {
                                "v1"
                            }
                        }
                    },
                    new List<object>()
                });
                this.assertEqual(A1, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object> {
                                "v2"
                            }
                        }
                    },
                    new List<object>()
                });
                this.assertEqual(A2, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object>()
                        },
                        new List<object> {
                            "b",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object>()
                });
                this.assertEqual(A3, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object> {
                                "v2"
                            }
                        },
                        new List<object> {
                            "b",
                            1,
                            new List<object> {
                                "v4"
                            }
                        }
                    },
                    new List<object>()
                });
                this.assertEqual(A4, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            3,
                            new List<object> {
                                "v5",
                                "v2"
                            }
                        }
                    },
                    new List<object>()
                });
            }
            
            public virtual object test_sync() {
                var X = new List<object> {
                    new List<object> {
                        new List<object> {
                            "x",
                            1,
                            new List<object>()
                        }
                    },
                    new List<object>()
                };
                var A = this.dvvset.create(this.dvvset.@new("v1"), "a");
                var Y = this.dvvset.create(this.dvvset.new_list(new List<object> {
                    "v2"
                }), "b");
                var A1 = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(A), new List<object> {
                    "v2"
                }), "a");
                var A3 = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(A1), new List<object> {
                    "v3"
                }), "b");
                var A4 = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(A1), new List<object> {
                    "v3"
                }), "c");
                var W = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            1,
                            new List<object>()
                        }
                    },
                    new List<object>()
                };
                var Z = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object> {
                                "v2",
                                "v1"
                            }
                        }
                    },
                    new List<object>()
                };
                this.assertEqual(this.dvvset.sync(new List<object> {
                    W,
                    Z
                }), new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object> {
                                "v2"
                            }
                        }
                    },
                    new List<object>()
                });
                this.assertEqual(this.dvvset.sync(new List<object> {
                    W,
                    Z
                }), this.dvvset.sync(new List<object> {
                    Z,
                    W
                }));
                this.assertEqual(this.dvvset.sync(new List<object> {
                    A,
                    A1
                }), this.dvvset.sync(new List<object> {
                    A1,
                    A
                }));
                this.assertEqual(this.dvvset.sync(new List<object> {
                    A4,
                    A3
                }), this.dvvset.sync(new List<object> {
                    A3,
                    A4
                }));
                this.assertEqual(this.dvvset.sync(new List<object> {
                    A4,
                    A3
                }), new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object>()
                        },
                        new List<object> {
                            "b",
                            1,
                            new List<object> {
                                "v3"
                            }
                        },
                        new List<object> {
                            "c",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object>()
                });
                this.assertEqual(this.dvvset.sync(new List<object> {
                    X,
                    A
                }), new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            1,
                            new List<object> {
                                "v1"
                            }
                        },
                        new List<object> {
                            "x",
                            1,
                            new List<object>()
                        }
                    },
                    new List<object>()
                });
                this.assertEqual(this.dvvset.sync(new List<object> {
                    X,
                    A
                }), this.dvvset.sync(new List<object> {
                    A,
                    X
                }));
                this.assertEqual(this.dvvset.sync(new List<object> {
                    X,
                    A
                }), this.dvvset.sync(new List<object> {
                    A,
                    X
                }));
                this.assertEqual(this.dvvset.sync(new List<object> {
                    A,
                    Y
                }), new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            1,
                            new List<object> {
                                "v1"
                            }
                        },
                        new List<object> {
                            "b",
                            1,
                            new List<object> {
                                "v2"
                            }
                        }
                    },
                    new List<object>()
                });
                this.assertEqual(this.dvvset.sync(new List<object> {
                    Y,
                    A
                }), this.dvvset.sync(new List<object> {
                    A,
                    Y
                }));
                this.assertEqual(this.dvvset.sync(new List<object> {
                    Y,
                    A
                }), this.dvvset.sync(new List<object> {
                    A,
                    Y
                }));
                this.assertEqual(this.dvvset.sync(new List<object> {
                    A,
                    X
                }), this.dvvset.sync(new List<object> {
                    X,
                    A
                }));
            }
            
            public virtual object test_sync_update() {
                // Mary writes v1 w/o VV
                var A0 = this.dvvset.create(this.dvvset.new_list(new List<object> {
                    "v1"
                }), "a");
                // Peter reads v1 with version vector (VV)
                var VV1 = this.dvvset.join(A0);
                // Mary writes v2 w/o VV
                var A1 = this.dvvset.update(this.dvvset.new_list(new List<object> {
                    "v2"
                }), A0, "a");
                // Peter writes v3 with VV from v1
                var A2 = this.dvvset.update(this.dvvset.new_list_with_history(VV1, new List<object> {
                    "v3"
                }), A1, "a");
                this.assertEqual(VV1, new List<object> {
                    new List<object> {
                        "a",
                        1
                    }
                });
                this.assertEqual(A0, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            1,
                            new List<object> {
                                "v1"
                            }
                        }
                    },
                    new List<object>()
                });
                this.assertEqual(A1, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object> {
                                "v2",
                                "v1"
                            }
                        }
                    },
                    new List<object>()
                });
                // now A2 should only have v2 and v3, since v3 was causally newer than v1
                this.assertEqual(A2, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            3,
                            new List<object> {
                                "v3",
                                "v2"
                            }
                        }
                    },
                    new List<object>()
                });
            }
            
            public virtual object test_event() {
                new List<object> {
                    A,
                    _
                } = this.dvvset.create(this.dvvset.@new("v1"), "a");
                this.assertEqual(this.dvvset.@event(A, "a", "v2"), new List<object> {
                    new List<object> {
                        "a",
                        2,
                        new List<object> {
                            "v2",
                            "v1"
                        }
                    }
                });
                this.assertEqual(this.dvvset.@event(A, "b", "v2"), new List<object> {
                    new List<object> {
                        "a",
                        1,
                        new List<object> {
                            "v1"
                        }
                    },
                    new List<object> {
                        "b",
                        1,
                        new List<object> {
                            "v2"
                        }
                    }
                });
            }
            
            public virtual object test_less() {
                var A = this.dvvset.create(this.dvvset.new_list("v1"), new List<object> {
                    "a"
                });
                var B = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(A), new List<object> {
                    "v2"
                }), "a");
                var B2 = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(A), new List<object> {
                    "v2"
                }), "b");
                var B3 = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(A), new List<object> {
                    "v2"
                }), "z");
                var C = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(B), new List<object> {
                    "v3"
                }), A, "c");
                var D = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(C), new List<object> {
                    "v4"
                }), B2, "d");
                this.assertTrue(this.dvvset.less(A, B));
                this.assertTrue(this.dvvset.less(A, C));
                this.assertTrue(this.dvvset.less(B, C));
                this.assertTrue(this.dvvset.less(B, D));
                this.assertTrue(this.dvvset.less(B2, D));
                this.assertTrue(this.dvvset.less(A, D));
                this.assertFalse(this.dvvset.less(B2, C));
                this.assertFalse(this.dvvset.less(B, B2));
                this.assertFalse(this.dvvset.less(B2, B));
                this.assertFalse(this.dvvset.less(A, A));
                this.assertFalse(this.dvvset.less(C, C));
                this.assertFalse(this.dvvset.less(D, B2));
                this.assertFalse(this.dvvset.less(B3, D));
            }
            
            public virtual object test_equal() {
                var A = Clock(new List<object> {
                    new List<object> {
                        "a",
                        4,
                        new List<object> {
                            "v5",
                            "v0"
                        }
                    },
                    new List<object> {
                        "b",
                        0,
                        new List<object>()
                    },
                    new List<object> {
                        "c",
                        1,
                        new List<object> {
                            "v3"
                        }
                    }
                }, new List<object> {
                    "v0"
                });
                var B = Clock(new List<object> {
                    new List<object> {
                        "a",
                        4,
                        new List<object> {
                            "v555",
                            "v0"
                        }
                    },
                    new List<object> {
                        "b",
                        0,
                        new List<object>()
                    },
                    new List<object> {
                        "c",
                        1,
                        new List<object> {
                            "v3"
                        }
                    }
                }, new List<object>());
                var C = Clock(new List<object> {
                    new List<object> {
                        "a",
                        4,
                        new List<object> {
                            "v5",
                            "v0"
                        }
                    },
                    new List<object> {
                        "b",
                        0,
                        new List<object>()
                    }
                }, new List<object> {
                    "v6",
                    "v1"
                });
                // compare only the causal history
                this.assertTrue(this.dvvset.equal(A, B));
                this.assertTrue(this.dvvset.equal(B, A));
                this.assertFalse(this.dvvset.equal(A, C));
                this.assertFalse(this.dvvset.equal(B, C));
            }
            
            public virtual object test_size() {
                Tuple.Create(this.assertEqual(1, this.dvvset.size(this.dvvset.new_list(new List<object> {
                    "v1"
                }))));
                var clock = Clock(new List<object> {
                    new List<object> {
                        "a",
                        4,
                        new List<object> {
                            "v5",
                            "v0"
                        }
                    },
                    new List<object> {
                        "b",
                        0,
                        new List<object>()
                    },
                    new List<object> {
                        "c",
                        1,
                        new List<object> {
                            "v3"
                        }
                    }
                }, new List<object> {
                    "v4",
                    "v1"
                });
                this.assertEqual(5, this.dvvset.size(clock));
            }
            
            public virtual object test_values() {
                var A = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object> {
                                "v0",
                                "v5"
                            }
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        },
                        new List<object> {
                            "c",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object> {
                        "v1"
                    }
                };
                var B = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object> {
                                "v0",
                                "v555"
                            }
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        },
                        new List<object> {
                            "c",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object>()
                };
                var C = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object>()
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        }
                    },
                    new List<object> {
                        "v1",
                        "v6"
                    }
                };
                this.assertEqual(this.dvvset.ids(A), new List<object> {
                    "a",
                    "b",
                    "c"
                });
                this.assertEqual(this.dvvset.ids(B), new List<object> {
                    "a",
                    "b",
                    "c"
                });
                this.assertEqual(this.dvvset.ids(C), new List<object> {
                    "a",
                    "b"
                });
                this.assertEqual(this.dvvset.values(A).OrderBy(_p_1 => _p_1).ToList(), new List<object> {
                    "v0",
                    "v1",
                    "v3",
                    "v5"
                });
                this.assertEqual(this.dvvset.values(B).OrderBy(_p_2 => _p_2).ToList(), new List<object> {
                    "v0",
                    "v3",
                    "v555"
                });
                this.assertEqual(this.dvvset.values(C).OrderBy(_p_3 => _p_3).ToList(), new List<object> {
                    "v1",
                    "v6"
                });
            }
            
            public virtual object test_ids_values() {
                var A = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object> {
                                "v0",
                                "v5"
                            }
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        },
                        new List<object> {
                            "c",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object> {
                        "v1"
                    }
                };
                var B = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object> {
                                "v0",
                                "v555"
                            }
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        },
                        new List<object> {
                            "c",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object>()
                };
                var C = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object>()
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        }
                    },
                    new List<object> {
                        "v1",
                        "v6"
                    }
                };
                this.assertEqual(this.dvvset.ids(A), new List<object> {
                    "a",
                    "b",
                    "c"
                });
                this.assertEqual(this.dvvset.ids(B), new List<object> {
                    "a",
                    "b",
                    "c"
                });
                this.assertEqual(this.dvvset.ids(C), new List<object> {
                    "a",
                    "b"
                });
                this.assertEqual(this.dvvset.values(A).OrderBy(_p_1 => _p_1).ToList(), new List<object> {
                    "v0",
                    "v1",
                    "v3",
                    "v5"
                });
                this.assertEqual(this.dvvset.values(B).OrderBy(_p_2 => _p_2).ToList(), new List<object> {
                    "v0",
                    "v3",
                    "v555"
                });
                this.assertEqual(this.dvvset.values(C).OrderBy(_p_3 => _p_3).ToList(), new List<object> {
                    "v1",
                    "v6"
                });
            }
        }
        
        static Module() {
            unittest.main();
        }
    }
}
